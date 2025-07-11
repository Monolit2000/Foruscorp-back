﻿using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckStatus;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged
{
    public class UpdateTruckTrackerIfChangedCommandHandler : IRequestHandler<UpdateTruckTrackerIfChangedCommand>
    {
        private readonly ITuckTrackingContext _context;
        private readonly ILogger<UpdateTruckTrackerIfChangedCommandHandler> _logger;
        private readonly TruckInfoManager _truckInfoManager;
        private readonly ISender _sender;

        public UpdateTruckTrackerIfChangedCommandHandler(
            ITuckTrackingContext context,
            ILogger<UpdateTruckTrackerIfChangedCommandHandler> logger,
            TruckInfoManager truckInfoManager,
            ISender sender)
        {
            _context = context;
            _logger = logger;
            _truckInfoManager = truckInfoManager;
            _sender = sender;
        }

        public async Task Handle(UpdateTruckTrackerIfChangedCommand request, CancellationToken cancellationToken)
        {
            var updates = request.TruckStatsUpdates
                .Where(u => u != null)
                .DistinctBy(u => u.TruckId)
                .ToList();

            if (!updates.Any())
            {
                _logger.LogWarning("No truck stats updates provided.");
                return;
            }

            // Группируем все TruckIds
            var truckIds = updates.Select(u => Guid.Parse(u.TruckId)).ToList();

            // Загружаем все трекеры одним запросом
            var trackers = await _context.TruckTrackers
                .Where(tt => truckIds.Contains(tt.TruckId))
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.CurrentRoute)
                .ToListAsync(cancellationToken);

            var statusCommands = new List<UpdateTruckStatusCommand>();

            foreach (var tracker in trackers)
            {
                var updateModel = updates.First(u => Guid.Parse(u.TruckId) == tracker.TruckId);

                bool locationChanged = _truckInfoManager.UpdateTruckLocationInfoIfChanged(updateModel);
                bool fuelChanged = _truckInfoManager.UpdateTruckIFuelnfoIfChanged(updateModel);
                bool engineChanged = _truckInfoManager.UpdateTruckEngineInfoIfChanged(updateModel);

                if (locationChanged)
                {
                    tracker.UpdateCurrentTruckLocation(
                        new GeoPoint(updateModel.Latitude, updateModel.Longitude),
                        updateModel.formattedLocation);
                    _logger.LogInformation("TruckId: {TruckId}, Location changed", tracker.TruckId);
                }

                if (fuelChanged)
                {
                    tracker.UpdateFuelStatus(updateModel.fuelPercents);
                    _logger.LogInformation("TruckId: {TruckId}, Fuel changed", tracker.TruckId);
                }

                if (engineChanged)
                {
                    statusCommands.Add(new UpdateTruckStatusCommand
                    {
                        TruckId = tracker.TruckId,
                        EngineStatus = updateModel.engineStateData?.Value ?? string.Empty
                    });
                    _logger.LogInformation("TruckId: {TruckId}, Engine status changed", tracker.TruckId);
                }
            }

            // Сохраняем все изменения разом
            await _context.SaveChangesAsync(cancellationToken);

            // Отправляем команды обновления статуса двигателя
            foreach (var cmd in statusCommands)
            {
                await _sender.Send(cmd, cancellationToken);
            }
        }
    }
}
