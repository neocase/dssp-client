﻿/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014-2017 Egelke BVBA
 *  Copyright (C) 2014 e-Contract.be BVBA
 *
 *  DSS-P client is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  DSS-P client is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with DSS-P client.  If not, see <http://www.gnu.org/licenses/>.
 */

using EContract.Dssp.Client.Proxy;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    public partial class DsspClient
    {
        /// <summary>
        /// Uploads a document to e-Contract, asynchronously.
        /// </summary>
        /// <see cref="UploadDocument"/>
        public async Task<DsspSession> UploadDocumentAsync(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var client = CreateDSSPClient();
            var request = CreateAsyncSignRequest(document, out var clientNonce);
            signResponse1 responseWrapper = await client.signAsync(request);
            return ProcessAsyncSignResponse(responseWrapper.SignResponse, clientNonce);
        }

        /// <summary>
        /// Uploads a document to e-Contract, asynchronously.
        /// </summary>
        /// <see cref="UploadDocumentFor2Step(Document)"/>
        public async Task<Dssp2StepSession> UploadDocumentFor2StepAsync(Document document)
        {
            return await UploadDocumentFor2StepAsync(document, null);
        }

        /// <summary>
        /// Uploads a document to e-Contract, asynchronously.
        /// </summary>
        /// <see cref="UploadDocumentFor2Step(Document, SignatureRequestProperties)"/>
        public async Task<Dssp2StepSession> UploadDocumentFor2StepAsync(Document document, SignatureRequestProperties properties)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (!(Signer?.HasPrivateKey ?? false && Signer?.PrivateKey is RSACryptoServiceProvider)) throw new InvalidOperationException("Singner must be set and have a private key");

            var client = CreateDSSPClient();
            var request = Create2StepSignRequest(document, properties);
            signResponse1 response = await client.signAsync(request);
            return Process2StepSignResponse(response.SignResponse);
        }

        /// <summary>
        /// Downloads the document that was uploaded before and signed via the BROWSER/POST protocol, asynchronously.
        /// </summary>
        /// <see cref="DownloadDocument(DsspSession)"/>
        public async Task<Document> DownloadDocumentAsync(DsspSession session)
        {
            if (session == null) throw new ArgumentNullException("session");

            var client = CreateDSSPClient(session);
            var downloadRequest = CreateDownloadRequest(session);
            pendingRequestResponse downloadResponseWrapper = await client.pendingRequestAsync(downloadRequest);
            return ProcessResponseWithSignedDoc(downloadResponseWrapper.SignResponse);
        }

        /// <summary>
        /// Downloads the document that was uploaded before and signed offline.
        /// </summary>
        /// <see cref="DownloadDocument(Dssp2StepSession)"/>
        public async Task<Document> DownloadDocumentAsync(Dssp2StepSession session)
        {
            if (session == null) throw new ArgumentNullException("session");

            var client = CreateDSSPClient();
            var downloadRequest = CreateDownloadRequest(session);
            signResponse1 downloadResponse = await client.signAsync(downloadRequest);
            return ProcessResponseWithSignedDoc(downloadResponse.SignResponse);
        }

        /// <summary>
        /// Validates the provided document via the e-contract service, asynchronously.
        /// </summary>
        /// <see cref="Verify"/>
        public async Task<SecurityInfo> VerifyAsync(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var client = CreateDSSPClient();
            var request = CreateVerifyRequest(document);
            verifyResponse responseWrapper = await client.verifyAsync(request);
            return ProcessVerifyResponse(responseWrapper.VerifyResponse1);
        }

        /// <summary>
        /// Add an eSeal to the document via the e-contract service.
        /// </summary>
        /// <see cref="Seal(Document, SignatureRequestProperties)"/>
        public async Task<Document> SealAsync(Document document, SignatureRequestProperties properties)
        {
            if (document == null) throw new ArgumentNullException("document");

            var client = CreateDSSPClient();
            var request = CreateSealRequest(document, properties);
            signResponse1 responseWrapper = await client.signAsync(request);
            return ProcessResponseWithSignedDoc(responseWrapper.SignResponse);
        }

    }
}
