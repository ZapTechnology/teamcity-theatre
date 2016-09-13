using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamCityTheatre.Core.ApplicationModels;
using TeamCityTheatre.Core.DataServices;
using TeamCityTheatre.Core.Models;
using TeamCityTheatre.Core.QueryServices;
using TeamCityTheatre.Core.QueryServices.Models;

namespace TeamCityTheatre.Application.QueryServices
{
    public class TileService : ITileService
    {
        private readonly IBuildDataService _buildDataService;

        public TileService(IBuildDataService buildDataService)
        {
            if (buildDataService == null)
            {
                throw new ArgumentNullException(nameof(buildDataService));
            }
            _buildDataService = buildDataService;
        }

        public async Task<TileData> GetLatestTileDataAsync(View view, Tile tile)
        {
            try
            {
                IEnumerable<IDetailedBuild> builds = await _buildDataService.GetBuildsOfBuildConfigurationAsync(tile.BuildConfigurationId, 20);
                return new TileData
                {
                    Label = tile.Label,
                    Builds = builds.GroupBy(b => b.BranchName)
                            .Select(buildsOfBranch => GenerateBuildInfo(buildsOfBranch))
                            .Take(view.DefaultNumberOfBranchesPerTile)
                            .ToList()
                };
            }
            catch (Exception)
            {
                return new TileData
                {
                    Label = tile.Label,
                    Builds = new List<IDetailedBuild>()
                };
            }
        }

        private static IDetailedBuild GenerateBuildInfo(IGrouping<string, IDetailedBuild> buildsOfBranch)
        {
            IDetailedBuild currentBuild = buildsOfBranch.OrderByDescending(b => b.StartDate).FirstOrDefault();
            if (HasFinished(currentBuild))
                return currentBuild;

            IDetailedBuild lastFinishedBuild = buildsOfBranch.OrderByDescending(b => b.StartDate).FirstOrDefault(b => HasFinished(b));


            return new Build
            {
                Id = currentBuild.Id,
                BuildConfigurationId = currentBuild.BuildConfigurationId,
                Agent = currentBuild.Agent,
                ArtifactDependencies = currentBuild.ArtifactDependencies,
                BranchName = currentBuild.BranchName,
                BuildConfiguration = currentBuild.BuildConfiguration,
                FinishDate = currentBuild.FinishDate,
                Href = currentBuild.Href,
                IsDefaultBranch = currentBuild.IsDefaultBranch,
                LastChanges = currentBuild.LastChanges,
                Number = currentBuild.Number,
                PercentageComplete = currentBuild.PercentageComplete,
                ElapsedSeconds = currentBuild.ElapsedSeconds,
                EstimatedTotalSeconds = currentBuild.EstimatedTotalSeconds,
                CurrentStageText = currentBuild.CurrentStageText,
                Properties = currentBuild.Properties,
                QueuedDate = currentBuild.QueuedDate,
                SnapshotDependencies = currentBuild.SnapshotDependencies,
                StartDate = currentBuild.StartDate,
                State = currentBuild.State,
                Status = lastFinishedBuild.Status,
                StatusText = lastFinishedBuild.StatusText,
                WebUrl = currentBuild.WebUrl
            };
        }

        private static bool HasFinished(IDetailedBuild b)
        {
            return b.State.Equals("finished", StringComparison.OrdinalIgnoreCase);
        }
    }
}