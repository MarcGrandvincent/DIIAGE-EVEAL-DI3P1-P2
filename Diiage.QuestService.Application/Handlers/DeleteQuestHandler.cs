using Diiage.QuestService.Application.Requests.Commands;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Repositories.Interfaces;
using MediatR;
using ProjectPierre.Common.Exceptions.Exceptions;

namespace Diiage.QuestService.Application.Handlers;

public class DeleteQuestHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteQuestCommand>
{
    private readonly IGenericRepository<QuestDao> _questRepository = unitOfWork.GetRepository<QuestDao>();
    
    public async Task Handle(DeleteQuestCommand request, CancellationToken cancellationToken)
    {
        var questDao = await _questRepository.GetFirstOrDefaultAsync(predicate: e => e.Id == request.Id, cancellationToken: cancellationToken);

        if (questDao is null)
            throw new NotFoundException();
        
        await _questRepository.DeleteAsync(questDao.Id, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }
}