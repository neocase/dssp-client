﻿using EContract.Dssp.Client;
using EContract.Dssp.Client.Proxy;
using forms_demo.Properties;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;

namespace forms_demo
{
    public partial class Signed : System.Web.UI.Page
    {

        DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Retrieve the content
                string signResponse = Request.Form.Get("SignResponse");

                //Retrieve the session
                DsspSession session = (DsspSession)Session["dsspSession"];

                // verify whether DsspSession is serializable
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, session);
                memoryStream.Seek(0, SeekOrigin.Begin);
                session = (DsspSession)binaryFormatter.Deserialize(memoryStream);

                Document signedDocument;
                try
                {
                    //Check if the content is valid, this isn't required but strongly advised.
                    NameIdentifierType newSigner = session.ValidateSignResponse(signResponse);

                    //Remove the DSS-P Session from the HTTP Session 
                    Session.Remove("dsspSession");

                    //download the signed document
                    signedDocument = dsspClient.DownloadDocument(session);

                    //You should save the signed document about here...
                    Session["signedDocument"] = signedDocument;

                    //For demo purposes, lets validate the signature.  This is purely optional
                    SecurityInfo securityInfo = dsspClient.Verify(signedDocument);

                    //Display some interesting info about the signed document
                    this.msg.Text = "signed document with timestamp valid until " + securityInfo.TimeStampValidity;
                    foreach(SignatureInfo signature in securityInfo.Signatures)
                    {
                        if (signature.SignerSubject == newSigner.Value)
                        {
                            this.signatures.Items.Add("New: Signed by " + signature.Signer.Subject + " on " + signature.SigningTime);
                        }
                        else
                        {
                            this.signatures.Items.Add("Signed by " + signature.Signer.Subject + " on " + signature.SigningTime);
                        }
                    }

                    this.view.Enabled = true;
                }
                catch (AuthorizationError error)
                {
                    //Failed, lets display the error
                    this.msg.Text = "authorization error: " + error.AttemptedSigner.Value;
                    this.view.Enabled = false;
                    return;
                }
                catch (RequestError error)
                {
                    //Failed, lets display the error
                    this.msg.Text = "signing error: " + error.Message;
                    this.view.Enabled = false;
                    return;
                }
            }
        }

        protected void home_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void view_Click(object sender, EventArgs e)
        {
            Document signedDocument = (Document) Session["signedDocument"];
            Response.ContentType = signedDocument.MimeType;
            Response.AppendHeader("Content-Disposition", "attachment;filename=signed.pdf");
            CopyStream(signedDocument.Content, Response.OutputStream);
            Response.End();
        }

        protected void seal_Click(object sender, EventArgs e)
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\certificate.p12"), "");

            Document sealedDocument = dsspClient.Seal((Document)Session["signedDocument"]);
            Session["signedDocument"] = sealedDocument;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}
