using Improbable.Gdk.GameObjectRepresentation;
using System.Collections.Generic;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Unity.Entities;

namespace Assets.Gamelogic
{
    public static class WorkerPlatform
    {
        public const string UnityClient = "UnityClient";
        public const string UnityGameLogic = "UnityGameLogic";
        public const string UnityWorker = "UnityGameLogic";
        //public const string AndroidClient = "AndroidClient";
        //public const string iOSClient = "iOSClient";

        public static readonly string[] AllWorkerAttributes =
        {
            UnityGameLogic,
            UnityClient,
            //AndroidClient,
            //iOSClient
        };
    }
}
