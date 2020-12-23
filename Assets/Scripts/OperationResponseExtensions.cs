using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public static class OperationResponseExtensions
    {
        public static bool CheckIsValidAndReport( this OperationResponse response, string ErrorMessage )
        {
            if( response.Parameters.ContainsKey( ParameterCode.WebRpcReturnCode ) && (byte)response.Parameters[ ParameterCode.WebRpcReturnCode ] == 41 )
            {
                MessagePopup.Show( $"{response.Parameters[ParameterCode.WebRpcReturnMessage]}: {ErrorMessage}", LogType.Warning );
                return false;
            }

            return true;
        }

        public static bool IsResponseToUriPath( this OperationResponse response, string path )
        {
            return response.Parameters.ContainsKey( ParameterCode.UriPath ) && response.Parameters[ ParameterCode.UriPath ].ToString() == path;
        }

    }
}
