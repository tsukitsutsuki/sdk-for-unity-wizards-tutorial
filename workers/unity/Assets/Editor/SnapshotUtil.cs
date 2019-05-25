using Assets.Gamelogic.Core;
using Assets.Gamelogic.EntityTemplate;
using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Gdk.Core;
using UnityEngine;

namespace Assets.Editor
{
    public static class SnapshotUtil
    {
        private static readonly System.Random rand = new System.Random();

        public static void AddPlayerSpawner(Snapshot snapshot)
        {
			var entity = EntityTemplateFactory.CreatePlayerSpawnerTemplate();
            snapshot.AddEntity(entity);
        }

        public static void AddTrees(Snapshot snapshot, Texture2D sampler, float sampleThreshold, int countAproximate, double edgeLength, float placementJitter)
        {
            var treeCountSqrt = Mathf.CeilToInt(Mathf.Sqrt(countAproximate));
            var spawnGridIntervals = edgeLength / treeCountSqrt;

            for (var z = 0; z < treeCountSqrt; z++)
            {
                var zProportion = z / (float)treeCountSqrt;

                for (var x = 0; x < treeCountSqrt; x++)
                {
                    var xProportion = x / (float)treeCountSqrt;
                    var xPixel = (int) (xProportion * sampler.width);
                    var zPixel = (int) (zProportion * sampler.height);
                    var sample = sampler.GetPixel(xPixel, zPixel).maxColorComponent;

                    if (sample > sampleThreshold && Random.value < sample)
                    {
                        var xJitter = Random.Range(-placementJitter, placementJitter);
                        var zJitter = Random.Range(-placementJitter, placementJitter);
                        Vector3d positionJitter = new Vector3d(xJitter, 0d, zJitter);

                        Vector3d offsetFromWorldRoot = new Vector3d(x * spawnGridIntervals, 0d, z * spawnGridIntervals);
                        Coordinates spawnPosition = new Coordinates(
                            (-edgeLength/2) + offsetFromWorldRoot.X + positionJitter.X,
                            0 + offsetFromWorldRoot.Y + positionJitter.Y,
                            (-edgeLength/2) + offsetFromWorldRoot.Z + positionJitter.Z
                        );
                        AddTree(snapshot, spawnPosition);
                    }
                }
            }
        }

        public static void AddTree(Snapshot snapshot, Coordinates position)
        {
            var spawnRotation = (uint)Mathf.CeilToInt((float)rand.NextDouble() * 360.0f);
            var entity = EntityTemplateFactory.CreateTreeTemplate(position, spawnRotation);
            snapshot.AddEntity(entity);
        }

        public static void AddHQs(Snapshot snapshot, Coordinates[] locations)
        {
            for (uint teamId = 0; teamId < locations.Length; teamId++)
            {
                var position = locations[teamId];
                var entity = EntityTemplateFactory.CreateHQTemplate(position, 0, teamId);
                snapshot.AddEntity(entity);
            }
        }

        public static void AddNPCsAroundHQs(Snapshot snapshot, Coordinates[] locations)
        {
            for (uint teamId = 0; teamId < locations.Length; teamId++)
            {
                SpawnNpcsAroundPosition(snapshot, locations[teamId], teamId);
            }
        }

        public static void SpawnNpcsAroundPosition(Snapshot snapshot, Coordinates position, uint team)
        {
            float totalNpcs = SimulationSettings.HQStartingWizardsCount + SimulationSettings.HQStartingLumberjacksCount;
            float radiusFromHQ = SimulationSettings.NPCSpawnDistanceToHQ;

            for (int i = 0; i < totalNpcs; i++)
            {
                float radians = (i / totalNpcs) * 2 * Mathf.PI;
                Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));
                offset *= radiusFromHQ;
                Coordinates coordinates = (position.ToVector3() + offset).ToCoordinates();

                EntityTemplate entity = null;
                if (i < SimulationSettings.HQStartingLumberjacksCount)
                {
                    entity = EntityTemplateFactory.CreateNPCLumberjackTemplate(coordinates, team);
                }
                else
                {
                    entity = EntityTemplateFactory.CreateNPCWizardTemplate(coordinates, team);
                }

                snapshot.AddEntity(entity);
            }
        }
    }
}