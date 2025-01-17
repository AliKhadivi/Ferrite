﻿//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using Ferrite.Data.Auth;
using Ferrite.Data.Account;
using Ferrite.Data;

namespace Ferrite.Services;

public interface IAuthService
{
    public Task<bool> SaveAppInfo(AppInfoDTO info);
    public Task<AppInfoDTO?> GetAppInfo(long authKeyId);
    public Task<bool> IsAuthorized(long authKeyId);
    /// <summary>
	/// Send the verification code for login
	/// </summary>
	/// <param name="phoneNumber">Phone number in international format</param>
	/// <param name="apiId">Application identifier</param>
	/// <param name="apiHash">Application secret hash</param>
	/// <param name="settings">Settings for the code type to send</param>
	/// <returns>The method returns an auth.SentCode object with information on the message sent.</returns>
    public Task<SentCodeDTO> SendCode(string phoneNumber, int apiId, string apiHash, CodeSettingsDTO settings);
    public Task<AuthorizationDTO> SignUp(long authKeyId, string phoneNumber, string phoneCodeHash, string firstName, string lastName);
    public Task<AuthorizationDTO> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode);
    public Task<LoggedOutDTO?> LogOut(long authKeyId);
    public Task<bool> ResetAuthorizations(long authKeyId);
    public Task<ExportedAuthorizationDTO> ExportAuthorization(long authKeyId, int dcId);
    public Task<AuthorizationDTO> ImportAuthorization(long userId, long authKeyId, byte[] bytes);
    public Task<bool> BindTempAuthKey(long tempAuthKeyId, long permAuthKeyId, int expiresAt);
    public Task<AuthorizationDTO> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken);
    public Task<AuthorizationDTO> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1);
    public Task<string> RequestPasswordRecovery();
    public Task<AuthorizationDTO> RecoverPassword(string code, PasswordInputSettingsDTO newSettings);
    public Task<SentCodeDTO> ResendCode(string phoneNumber, string phoneCodeHash);
    public Task<bool> CancelCode(string phoneNumber, string phoneCodeHash);
    public Task<bool> DropTempAuthKeys(long authKeyId, ICollection<long> exceptAuthKeys);
    public Task<LoginTokenDTO> ExportLoginToken(long authKeyId, long sessionId, int apiId, string apiHash, ICollection<long> exceptIds);
    public Task<LoginTokenDTO> ImportLoginToken(byte[] token);
    public Task<AppInfoDTO?> AcceptLoginToken(long authKeyId, byte[] token);
    public Task<bool> CheckRecoveryPassword(string code);
}

