using Diiage.QuestService.Application.Requests.Commands;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Models;
using Diiage.QuestService.Repositories.Interfaces;
using Diiage.QuestService.Repositories.Transactions;
using MapsterMapper;
using MediatR;

namespace Diiage.QuestService.Application.Handlers;

public class CreateQuestHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateQuestCommand, Quest>
{
    private readonly IGenericRepository<QuestDao> _questRepository = unitOfWork.GetRepository<QuestDao>();
    
    public async Task<Quest> Handle(CreateQuestCommand request, CancellationToken cancellationToken)
    {
        await using var tx = _questRepository.BeginTransaction();
        
        var questDao = new QuestDao
        {
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            IsActive = request.IsActive,
            Reward = request.Reward,
            TargetCount = request.TargetCount,
        };

        _questRepository.Add(questDao);
        await unitOfWork.SaveAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        
        return mapper.Map<QuestDao, Quest>(questDao);
    }
}