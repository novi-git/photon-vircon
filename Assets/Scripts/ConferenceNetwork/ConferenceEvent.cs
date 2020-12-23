using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub
{
    namespace Conference
    {
        public class ConferenceEvent
        {
            public const byte UpdatePosition = 10;
            public const byte UpdatePositionAndRotation = 11;
            public const byte UpdateTransform = 12;
            public const byte UpdateXRSystem = 13;
            public const byte EnterPortal = 14;
          

            public const byte SandboxRpc = 50;
            public const byte SandboxJoin = 51;

            public const byte RequestPrivateCall = 100;
            public const byte AcceptPrivateCall = 101;
            public const byte DeclinePrivateCall = 102;
            public const byte CancelPrivateCallRequest = 103;

            public const byte StageSpeakerCustomization = 200;
            public const byte StagePresentationUri = 201;
            public const byte StageEmotes = 202;
            public const byte StageScoreBoard = 203; // Score Board for Team Building

        }
    }
}
