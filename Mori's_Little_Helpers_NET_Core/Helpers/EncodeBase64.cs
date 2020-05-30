// ===============================
//
// COMPANY:         DMG MORI Software Solutions
// AUTHOR:          Moritz Schweighofer, Software Developer Control Systems Integrations
// CREATE DATE:     05-11-2019
//
// ===============================
//
// SVN-REVISION:    $Revision:  $
// SVN-AUTHOR:      $Author:  $
// SVN-DATE:        $Date:  $
//
// ===============================
//
// PURPOSE:         Class to Encode a Username and Password in Base64
// SPECIAL NOTES:
//
// ===============================
//
// Change History:
//
//==================================


using System;
using System.Text;

namespace Schweigm_NETCore_Helpers
{
    public class EncodeUsernamePasswordBase64
    {

        /// <summary>
        /// This static Function Encodes the Username and Password so the Modules can read it
        /// </summary>
        public static string Encode(string username, string password)
        {
            try
            {
                var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password)); //don't ask
                return encoded;
            }
            catch
            {
                return "";
            }
        }
    }
}