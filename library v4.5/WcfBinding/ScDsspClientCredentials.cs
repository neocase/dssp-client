﻿/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
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

using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

namespace EContract.Dssp.Client.WcfBinding
{
    internal class ScDsspClientCredentials : ClientCredentials
    {
        internal BinarySecretSecurityToken Token { get; private set; }

        public ScDsspClientCredentials(string id, byte[] derivedKey)
        {
            this.Token = new BinarySecretSecurityToken(id, derivedKey);
        }

        private ScDsspClientCredentials(ScDsspClientCredentials credentials)
        {
            this.Token = new BinarySecretSecurityToken(credentials.Token.Id, credentials.Token.GetKeyBytes());
        }

        protected override ClientCredentials CloneCore()
        {
            return new ScDsspClientCredentials(this);
        }

        public override System.IdentityModel.Selectors.SecurityTokenManager CreateSecurityTokenManager()
        {
            return new ScDsspClientCredentialsSecurityTokenManager(this);
        }
    }
}