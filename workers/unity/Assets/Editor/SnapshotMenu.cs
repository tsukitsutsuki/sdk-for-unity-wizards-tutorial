using Assets.Gamelogic;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.PlayerLifecycle;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    [InitializeOnLoad]
    public static class SnapshotMenu
    {
        static SnapshotMenu()
        {
        }

        [MenuItem("Improbable/Snapshots/Generate Default Snapshot %#&w")]
        private static void GenerateSnapshotDefault()
        {
            var path = Application.dataPath + "/../../../snapshots/default.snapshot";
            var snapshot = new Snapshot();
            AddPlayerSpawner(snapshot);
            SnapshotDefault.Build(snapshot);
            snapshot.WriteToFile(path);
        }

        private static void AddPlayerSpawner(Snapshot snapshot)
        {
            var metadata = new Metadata.Snapshot { EntityType = "PlayerCreator" };

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = Coordinates.Zero }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new PlayerCreator.Snapshot(), WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            snapshot.AddEntity(template);
        }
    }
}
