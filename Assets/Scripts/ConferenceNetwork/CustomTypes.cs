namespace SpaceHub.Conference
{
    using UnityEngine;
    using Photon.Realtime;
    using ExitGames.Client.Photon;
    using Photon.Chat;

    /// <summary>
    /// Internally used class, containing de/serialization methods for various Unity-specific classes.
    /// Adding those to the Photon serialization protocol allows you to send them in events, etc.
    /// </summary>
    public static class CustomTypes
    {
        static bool m_HasRegistered = false;

        /// <summary>Register</summary>
        internal static void Register()
        {
            if( m_HasRegistered == true )
            {
                return;
            }

            PhotonPeer.RegisterType( typeof( Vector2 ), (byte)'W', SerializeVector2, DeserializeVector2 );
            PhotonPeer.RegisterType( typeof( Vector3 ), (byte)'V', SerializeVector3, DeserializeVector3 );
            PhotonPeer.RegisterType( typeof( Quaternion ), (byte)'Q', SerializeQuaternion, DeserializeQuaternion );
            PhotonPeer.RegisterType( typeof( Color ), (byte)'Y', SerializeColor, DeserializeColor );
            PhotonPeer.RegisterType( typeof( CustomizationObject.Data ), (byte)'C', SerializeAvatarCustomization, DeserializeAvatarCustomization );

            ChatPeer.RegisterType( typeof( Vector2 ), (byte)'W', SerializeVector2, DeserializeVector2 );
            ChatPeer.RegisterType( typeof( Vector3 ), (byte)'V', SerializeVector3, DeserializeVector3 );
            ChatPeer.RegisterType( typeof( Quaternion ), (byte)'Q', SerializeQuaternion, DeserializeQuaternion );
            ChatPeer.RegisterType( typeof( Color ), (byte)'Y', SerializeColor, DeserializeColor );

            m_HasRegistered = true;
        }


        #region Custom De/Serializer Methods

        public static readonly byte[] memCustomization = new byte[ 5 + 3 * 3 * 4 + 3 * 4 ];

        public static byte[] SerializeAvatarCustomization( object customobject )
        {
            CustomizationObject.Data data = (CustomizationObject.Data)customobject;

            var output = new byte[ 5 + data.ColorValues.Length * 3 + data.FloatValues.Length * 4 ];
            output[ 0 ] = data.PackIndex;
            output[ 1 ] = data.CategoryIndex;
            output[ 2 ] = data.AssetIndex;
            output[ 3 ] = (byte)data.ColorValues.Length;
            output[ 4 ] = (byte)data.FloatValues.Length;

            int index = 5;
            for( int i = 0; i < data.ColorValues.Length; ++i )
            {
                output[ index + 0 ] = data.ColorValues[ i ].r;
                output[ index + 1 ] = data.ColorValues[ i ].g;
                output[ index + 2 ] = data.ColorValues[ i ].b;
                index += 3;
            }

            return output;
            /*
            int index = 0;
            short size = 0;
            lock( memCustomization )
            {
                byte[] bytes = memCustomization;

                Protocol.Serialize( data.PackIndex, bytes, ref index );
                Protocol.Serialize( data.CategoryIndex, bytes, ref index );
                Protocol.Serialize( data.AssetIndex, bytes, ref index );
                Protocol.Serialize( (byte)data.ColorValues.Length, bytes, ref index );
                Protocol.Serialize( (byte)data.FloatValues.Length, bytes, ref index );

                Debug.Log( index );

                outStream.Write( bytes, 0, 5 );
                size += 5;

                for( int i = 0; i < data.ColorValues.Length; ++i )
                {
                    short serializeSize = SerializeColor( outStream, data.ColorValues[ i ] );
                    size += serializeSize;
                    index += serializeSize;
                    Debug.Log( index );
                }

                for( int i = 0; i < data.FloatValues.Length; ++i )
                {
                    Protocol.Serialize( data.FloatValues[ i ], bytes, ref index );
                    size += 4;
                }
            }

            return size;*/
        }

        public static object DeserializeAvatarCustomization( byte[] data )
        {
            CustomizationObject.Data output = new CustomizationObject.Data();
            output.PackIndex = data[ 0 ];
            output.CategoryIndex = data[ 1 ];
            output.AssetIndex = data[ 2 ];
            var colorsLength = (byte)data[ 3 ];
            var floatsLength = (byte)data[ 4 ];
            output.ColorValues = new Color32[ colorsLength ];
            output.FloatValues = new float[ floatsLength ];

            int index = 5;
            for( int i = 0; i < colorsLength; ++i )
            {
                output.ColorValues[ i ] = new Color32( data[ index + 0 ], data[ index + 1 ], data[ index + 2 ], 255 );
                index += 3;
            }

            return output;
            /*lock( memVector3 )
            {
                inStream.Read( memCustomization, 0, 3 * 4 );
                data.PackIndex = memCustomization[ 0 ];
                data.CategoryIndex = memCustomization[ 1 ];
                data.AssetIndex = memCustomization[ 2 ];
                var colorsLength = (byte)memCustomization[ 3 ];
                var floatsLength = (byte)memCustomization[ 4 ];

                data.ColorValues = new Color[ colorsLength ];
                data.FloatValues = new float[ floatsLength ];
                int index = 5;

                for( int i = 0; i < colorsLength; ++i )
                {
                    data.ColorValues[ i ] = (Color)DeserializeColor( inStream, 3 * 4 );
                    index += 3 * 4;
                }

                for( int i = 0; i < floatsLength; ++i )
                {
                    Protocol.Deserialize( out data.FloatValues[ i ], memCustomization, ref index );
                } 
            }

            return data;*/
        }

        public static readonly byte[] memVector3 = new byte[ 3 * 4 ];

        private static short SerializeVector3( StreamBuffer outStream, object customobject )
        {
            Vector3 vo = (Vector3)customobject;

            int index = 0;
            lock( memVector3 )
            {
                byte[] bytes = memVector3;
                Protocol.Serialize( vo.x, bytes, ref index );
                Protocol.Serialize( vo.y, bytes, ref index );
                Protocol.Serialize( vo.z, bytes, ref index );
                outStream.Write( bytes, 0, 3 * 4 );
            }

            return 3 * 4;
        }

        private static object DeserializeVector3( StreamBuffer inStream, short length )
        {
            Vector3 vo = new Vector3();
            lock( memVector3 )
            {
                inStream.Read( memVector3, 0, 3 * 4 );
                int index = 0;
                Protocol.Deserialize( out vo.x, memVector3, ref index );
                Protocol.Deserialize( out vo.y, memVector3, ref index );
                Protocol.Deserialize( out vo.z, memVector3, ref index );
            }

            return vo;
        }

        public static readonly byte[] memColor = new byte[ 3 * 4 ];

        private static short SerializeColor( StreamBuffer outStream, object customobject )
        {
            Color vo = (Color)customobject;

            int index = 0;
            lock( memColor )
            {
                byte[] bytes = memColor;
                Protocol.Serialize( vo.r, bytes, ref index );
                Protocol.Serialize( vo.g, bytes, ref index );
                Protocol.Serialize( vo.b, bytes, ref index );
                outStream.Write( bytes, 0, 3 * 4 );
            }

            return 3 * 4;
        }

        private static object DeserializeColor( StreamBuffer inStream, short length )
        {
            Color vo = new Color();
            lock( memColor )
            {
                inStream.Read( memColor, 0, 3 * 4 );
                int index = 0;
                Protocol.Deserialize( out vo.r, memColor, ref index );
                Protocol.Deserialize( out vo.g, memColor, ref index );
                Protocol.Deserialize( out vo.b, memColor, ref index );
            }

            return vo;
        }


        public static readonly byte[] memVector2 = new byte[ 2 * 4 ];

        private static short SerializeVector2( StreamBuffer outStream, object customobject )
        {
            Vector2 vo = (Vector2)customobject;
            lock( memVector2 )
            {
                byte[] bytes = memVector2;
                int index = 0;
                Protocol.Serialize( vo.x, bytes, ref index );
                Protocol.Serialize( vo.y, bytes, ref index );
                outStream.Write( bytes, 0, 2 * 4 );
            }

            return 2 * 4;
        }

        private static object DeserializeVector2( StreamBuffer inStream, short length )
        {
            Vector2 vo = new Vector2();
            lock( memVector2 )
            {
                inStream.Read( memVector2, 0, 2 * 4 );
                int index = 0;
                Protocol.Deserialize( out vo.x, memVector2, ref index );
                Protocol.Deserialize( out vo.y, memVector2, ref index );
            }

            return vo;
        }


        public static readonly byte[] memQuarternion = new byte[ 4 * 4 ];

        private static short SerializeQuaternion( StreamBuffer outStream, object customobject )
        {
            Quaternion o = (Quaternion)customobject;

            lock( memQuarternion )
            {
                byte[] bytes = memQuarternion;
                int index = 0;
                Protocol.Serialize( o.w, bytes, ref index );
                Protocol.Serialize( o.x, bytes, ref index );
                Protocol.Serialize( o.y, bytes, ref index );
                Protocol.Serialize( o.z, bytes, ref index );
                outStream.Write( bytes, 0, 4 * 4 );
            }

            return 4 * 4;
        }

        private static object DeserializeQuaternion( StreamBuffer inStream, short length )
        {
            Quaternion o = new Quaternion();

            lock( memQuarternion )
            {
                inStream.Read( memQuarternion, 0, 4 * 4 );
                int index = 0;
                Protocol.Deserialize( out o.w, memQuarternion, ref index );
                Protocol.Deserialize( out o.x, memQuarternion, ref index );
                Protocol.Deserialize( out o.y, memQuarternion, ref index );
                Protocol.Deserialize( out o.z, memQuarternion, ref index );
            }

            return o;
        }

        #endregion
    }
}