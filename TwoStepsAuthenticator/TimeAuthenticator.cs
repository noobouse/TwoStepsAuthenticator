﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStepsAuthenticator {
    
    /// <summary>
    /// Implementation of rfc6238 Time-Based One-Time Password Algorithm
    /// </summary>
    public class TimeAuthenticator : Authenticator {
        private readonly Func<DateTime> NowFunc;

        public TimeAuthenticator(Func<DateTime> nowFunc = null) {
            this.NowFunc = (nowFunc == null) ? () => DateTime.Now : nowFunc;
        }

        /// <summary>
        /// Generates One-Time Password.
        /// </summary>
        /// <param name="secret">Shared Secret</param>
        /// <returns>OTP</returns>
        public string GetCode(string secret) {
            return GetCode(secret, NowFunc());
        }

        /// <summary>
        /// Generates One-Time Password.
        /// </summary>
        /// <param name="secret">Shared Secret</param>
        /// <param name="date">Time to use as challenge</param>
        /// <returns>OTP</returns>
        public string GetCode(string secret, DateTime date) {
            TimeSpan ts = (date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var interval = (long)ts.TotalSeconds / 30;

            return GetCodeInternal(secret, interval);
        }

        /// <summary>
        /// Checks if the passed code is valid.
        /// </summary>
        /// <param name="secret">Shared Secret</param>
        /// <param name="code">OTP</param>
        /// <returns>true if code matches</returns>
        public bool CheckCode(string secret, string code) {
            DateTime successfulTime = DateTime.MinValue;

            return CheckCode(secret, code, out successfulTime);
        }

        /// <summary>
        /// Checks if the passed code is valid.
        /// </summary>
        /// <param name="secret">Shared Secret</param>
        /// <param name="code">OTP</param>
        /// <param name="usedDateTime">Matching time if successful</param>
        /// <returns>true if code matches</returns>
        public bool CheckCode(string secret, string code, out DateTime usedDateTime) {
            var baseTime = NowFunc();
            DateTime successfulTime = DateTime.MinValue;

            // We need to do this in constant time
            var codeMatch = false;
            for (int i = -2; i <= 1; i++) {
                var checkTime = baseTime.AddSeconds(30 * i);
                if (ConstantTimeEquals(GetCode(secret, checkTime), code)) {
                    codeMatch = true;
                    successfulTime = checkTime;
                }
            }

            usedDateTime = successfulTime;
            return codeMatch;
        }
    }
}
