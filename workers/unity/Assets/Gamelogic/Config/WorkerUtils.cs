using Improbable.Gdk.GameObjectRepresentation;
using System.Collections.Generic;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Unity.Entities;
using Improbable.Gdk.GameObjectCreation;

namespace Assets.Gamelogic
{
    public static class WorkerUtils
    {
        public static void AddClientSystems(World world)
        {
            TransformSynchronizationHelper.AddClientSystems(world);
            PlayerLifecycleHelper.AddClientSystems(world);
            GameObjectRepresentationHelper.AddSystems(world);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(world);
        }

        public static void AddGameLogicSystems(World world)
        {
            TransformSynchronizationHelper.AddServerSystems(world);
            PlayerLifecycleHelper.AddServerSystems(world);
            GameObjectRepresentationHelper.AddSystems(world);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(world);
        }
    }
}
