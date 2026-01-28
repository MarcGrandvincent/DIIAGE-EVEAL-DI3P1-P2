using Diiage.QuestService.Application.Requests.Commands;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Models;
using Diiage.QuestService.Repositories.Interfaces;
using Diiage.QuestService.Repositories.Transactions;
using MapsterMapper;
using MediatR;
using ProjectPierre.Common.Exceptions.Exceptions;

namespace Diiage.QuestService.Application.Handlers;

public class UpdateQuestHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateQuestCommand, Quest>
{
    private readonly IGenericRepository<QuestDao> _questRepository = unitOfWork.GetRepository<QuestDao>();
    
    public async Task<Quest> Handle(UpdateQuestCommand request, CancellationToken cancellationToken)
    {
        await using var tx = _questRepository.BeginTransaction();
        
        var questDao = await _questRepository.GetFirstOrDefaultAsync(predicate: e => e.Id == request.Id, cancellationToken: cancellationToken);

        if (questDao is null)
            throw new NotFoundException();
        
        questDao.Code = request.Code;
        questDao.Title = request.Title;
        questDao.Description = request.Description;
        questDao.Type = request.Type;
        questDao.StartAt = request.StartAt;
        questDao.EndAt = request.EndAt;
        questDao.IsActive = request.IsActive;
        questDao.Reward = request.Reward;
        questDao.TargetCount = request.TargetCount;

        _questRepository.Update(questDao);
        await unitOfWork.SaveAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        
        return mapper.Map<QuestDao, Quest>(questDao);
    }
}

