/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014 Ingo Herbote
 * http://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Core.Services
{
    using System.Data;
    using System.Linq;
    using System.Web.Security;

    using YAF.Classes;
    using YAF.Classes.Data;
    using YAF.Core.Model;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.EventProxies;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;
    using YAF.Utils.Helpers;

    /// <summary>
    /// Single Sign On User Class to handle Twitter and Facebook Logins
    /// </summary>
    public class YafSingleSignOnUser
    {
        /// <summary>
        /// Generates the oAUTH callback login URL.
        /// </summary>
        /// <param name="authService">The AUTH service.</param>
        /// <param name="generatePopUpUrl">if set to <c>true</c> [generate pop up URL].</param>
        /// <param name="connectCurrentUser">if set to <c>true</c> [connect current user].</param>
        /// <returns>
        /// Returns the login Url
        /// </returns>
        public static string GenerateLoginUrl([NotNull] AuthService authService, [NotNull] bool generatePopUpUrl, [CanBeNull] bool connectCurrentUser = false)
        {
            switch (authService)
            {
                case AuthService.twitter:
                    {
                        return new Auth.Twitter().GenerateLoginUrl(generatePopUpUrl, connectCurrentUser);
                    }

                case AuthService.facebook:
                    {
                        return new Auth.Facebook().GenerateLoginUrl(generatePopUpUrl, connectCurrentUser);
                    }

                case AuthService.google:
                    {
                        return new Auth.Google().GenerateLoginUrl(generatePopUpUrl, connectCurrentUser);
                    }

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// The send registration notification email.
        /// </summary>
        /// <param name="user">The Membership User.</param>
        /// <param name="userID">The user identifier.</param>
        public static void SendRegistrationNotificationEmail([NotNull] MembershipUser user, [NotNull] int userID)
        {
            var emails = YafContext.Current.Get<YafBoardSettings>().NotificationOnUserRegisterEmailList.Split(';');

            var notifyAdmin = new YafTemplateEmail();

            var subject =
                YafContext.Current.Get<ILocalization>()
                    .GetText("COMMON", "NOTIFICATION_ON_USER_REGISTER_EMAIL_SUBJECT")
                    .FormatWith(YafContext.Current.Get<YafBoardSettings>().Name);

            notifyAdmin.TemplateParams["{adminlink}"] = YafBuildLink.GetLinkNotEscaped(
                ForumPages.admin_edituser,
                true,
                "u={0}",
                userID);
            notifyAdmin.TemplateParams["{user}"] = user.UserName;
            notifyAdmin.TemplateParams["{email}"] = user.Email;
            notifyAdmin.TemplateParams["{forumname}"] = YafContext.Current.Get<YafBoardSettings>().Name;

            string emailBody = notifyAdmin.ProcessTemplate("NOTIFICATION_ON_USER_REGISTER");

            foreach (string email in emails.Where(email => email.Trim().IsSet()))
            {
                YafContext.Current.GetRepository<Mail>()
                    .Create(YafContext.Current.Get<YafBoardSettings>().ForumEmail, email.Trim(), subject, emailBody);
            }
        }

        /// <summary>
        /// Sends a spam bot notification to admins.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userId">The user id.</param>
        public static void SendSpamBotNotificationToAdmins([NotNull] MembershipUser user, int userId)
        {
            // Get Admin Group ID
            var adminGroupID = 1;

            foreach (DataRow dataRow in
                LegacyDb.group_list(YafContext.Current.Get<YafBoardSettings>().BoardID, null)
                    .Rows.Cast<DataRow>()
                    .Where(
                        dataRow =>
                        !dataRow["Name"].IsNullOrEmptyDBField() && dataRow.Field<string>("Name") == "Administrators"))
            {
                adminGroupID = dataRow["GroupID"].ToType<int>();
                break;
            }

            using (DataTable dt = LegacyDb.user_emails(YafContext.Current.Get<YafBoardSettings>().BoardID, adminGroupID))
            {
                foreach (DataRow row in dt.Rows)
                {
                    var emailAddress = row.Field<string>("Email");

                    if (!emailAddress.IsSet())
                    {
                        continue;
                    }

                    var notifyAdmin = new YafTemplateEmail();

                    string subject =
                        YafContext.Current.Get<ILocalization>()
                            .GetText("COMMON", "NOTIFICATION_ON_BOT_USER_REGISTER_EMAIL_SUBJECT")
                            .FormatWith(YafContext.Current.Get<YafBoardSettings>().Name);

                    notifyAdmin.TemplateParams["{adminlink}"] = YafBuildLink.GetLinkNotEscaped(
                        ForumPages.admin_edituser,
                        true,
                        "u={0}",
                        userId);
                    notifyAdmin.TemplateParams["{user}"] = user.UserName;
                    notifyAdmin.TemplateParams["{email}"] = user.Email;
                    notifyAdmin.TemplateParams["{forumname}"] = YafContext.Current.Get<YafBoardSettings>().Name;

                    string emailBody = notifyAdmin.ProcessTemplate("NOTIFICATION_ON_BOT_USER_REGISTER");

                    YafContext.Current.GetRepository<Mail>()
                        .Create(YafContext.Current.Get<YafBoardSettings>().ForumEmail, emailAddress, subject, emailBody);
                }
            }
        }

        /// <summary>
        /// Do login and set correct flag
        /// </summary>
        /// <param name="authService">The AUTH service.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userID">The user ID.</param>
        /// <param name="doLogin">if set to <c>true</c> [do login].</param>
        public static void LoginSuccess([NotNull] AuthService authService, [CanBeNull] string userName, [NotNull] int userID, [NotNull] bool doLogin)
        {
            // Add Flag to User that indicates with what service the user is logged in
            LegacyDb.user_update_single_sign_on_status(userID, authService);

            if (!doLogin)
            {
                return;
            }

            FormsAuthentication.SetAuthCookie(userName, true);

            YafContext.Current.Get<IRaiseEvent>().Raise(new SuccessfulUserLoginEvent(userID));
        }
    }
}