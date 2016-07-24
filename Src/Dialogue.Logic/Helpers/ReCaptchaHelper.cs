using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace Dialogue.Logic.Helpers
{
    /// <summary>
    /// Helper class used for validating recaptcha encoded responses.
    /// <see href="http://stackoverflow.com/questions/27764692/validating-recaptcha-2-no-captcha-recaptcha-in-asp-nets-server-side"/>
    /// </summary>
    public class ReCaptchaHelper
    {
        /// <summary>
        /// Validates the specified encoded response.
        /// </summary>
        /// <param name="encodedResponse">The encoded response.</param>
        /// <param name="privateKey">The private key.</param>
        /// <returns>A value specifying whether the given encoded response is valid.</returns>
        public static bool Validate(string encodedResponse, string privateKey)
        {
            WebClient client = new WebClient();
            string googleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", privateKey, encodedResponse));
            ReCaptchaHelper captchaResponse = JsonConvert.DeserializeObject<ReCaptchaHelper>(googleReply);
            return captchaResponse.Success;
        }

        /// <summary>
        /// Gets a value indicating whether this the validation was a success.
        /// </summary>
        public bool Success
        {
            get
            {
                return m_Success == "True";
            }
        }

        /// <summary>
        /// The success string
        /// </summary>
        [JsonProperty("success")]
        private string m_Success;

        /// <summary>
        /// Gets or sets the error codes.
        /// </summary>
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}