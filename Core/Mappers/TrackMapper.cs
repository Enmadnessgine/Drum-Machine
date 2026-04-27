using System;
using System.Linq;
using System.Text;
using Drum_Machine.Models;
using Drum_Machine.Data.Entities;

namespace Drum_Machine.Core.Mappers
{
    public static class TrackMapper
    {
        public static TrackEntity ToEntity(DrumTrack model, int projectId)
        {
            return new TrackEntity
            {
                Name = model.Name,
                SamplePath = model.SamplePath,
                Volume = model.Volume,
                ProjectId = projectId,
                StepsData = string.Join("", model.Steps.Select(s => s ? "1" : "0"))
            };
        }

        public static DrumTrack ToModel(TrackEntity entity, int stepCount)
        {
            var track = new DrumTrack(entity.Name, stepCount)
            {
                SamplePath = entity.SamplePath,
                Volume = entity.Volume
            };

            for (int i = 0; i < Math.Min(stepCount, entity.StepsData.Length); i++)
            {
                track.Steps[i] = entity.StepsData[i] == '1';
            }

            return track;
        }
    }
}