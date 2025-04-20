using Azure.Core;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Api.Services
{
    public class StatusService : IStatusService
    {
        private readonly ILogger<StatusService> _logger;
        private readonly ApplicationContext _context;

        public StatusService(ILogger<StatusService> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public StatusDto? CreateStatus(long boardId, CreateOrUpdateStatusRequest request)
        {
            var board = _context.Boards.Find(boardId);

            if (board == null)
            {
                return null;
            }

            var status = new Status()
            {
                Board = board,
                Weight = request.Weight,
                Name = request.Name,
                ColorHex = request.ColorHex,
                TaskLimit = request.MaxTasks.HasValue ? request.MaxTasks.Value : 0,
            };

            _context.Add(status);
            _context.SaveChanges();

            return new StatusDto()
            {
                Name = request.Name,
                ColorHex = status.ColorHex,
                Id = status.Id,
                MaxTasks = status.TaskLimit > 0 ? status.TaskLimit : null,
                Weight = status.Weight
            };
        }

        public bool DeleteStatus(long statusId)
        {
            var status = _context.Statuses.Find(statusId);

            if (status == null || _context.Tasks.Any(e => e.StatusId == statusId))
            {
                return false;
            }

            _context.Remove(status);
            _context.SaveChanges();

            return true;
        }

        public StatusDto? UpdateStatus(long statusId, CreateOrUpdateStatusRequest request)
        {
            var status = _context.Statuses.Find(statusId);

            if (status == null)
            {
                return null;
            }

            status.Weight = request.Weight;
            status.Name = request.Name;
            status.ColorHex = request.ColorHex;
            status.TaskLimit = request.MaxTasks ?? 0;

            _context.Update(status);
            _context.SaveChanges();

            return new StatusDto()
            {
                Name = request.Name,
                ColorHex = status.ColorHex,
                Id = status.Id,
                MaxTasks = status.TaskLimit > 0 ? status.TaskLimit : null,
                Weight = status.Weight
            };
        }
    }
}