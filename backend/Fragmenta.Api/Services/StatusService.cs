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

        public async Task<StatusDto?> CreateStatusAsync(long boardId, CreateOrUpdateStatusRequest request)
        {
            var board = await _context.Boards.FindAsync(boardId);

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

            await _context.AddAsync(status);
            await _context.SaveChangesAsync();

            return new StatusDto()
            {
                Name = request.Name,
                ColorHex = status.ColorHex,
                Id = status.Id,
                MaxTasks = status.TaskLimit > 0 ? status.TaskLimit : null,
                Weight = status.Weight
            };
        }

        public async Task<bool> DeleteStatusAsync(long statusId)
        {
            var status = await _context.Statuses.FindAsync(statusId);

            if (status == null || await _context.Tasks.AnyAsync(e => e.StatusId == statusId))
            {
                return false;
            }

            _context.Remove(status);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<StatusDto?> UpdateStatusAsync(long statusId, CreateOrUpdateStatusRequest request)
        {
            var status = await _context.Statuses.FindAsync(statusId);

            if (status == null)
            {
                return null;
            }

            status.Weight = request.Weight;
            status.Name = request.Name;
            status.ColorHex = request.ColorHex;
            status.TaskLimit = request.MaxTasks ?? 0;

            _context.Update(status);
            await _context.SaveChangesAsync();

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