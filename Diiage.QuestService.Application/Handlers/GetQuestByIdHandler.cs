using Diiage.QuestService.Application.Requests.Queries;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Models;
using Diiage.QuestService.Repositories.Interfaces;
using MapsterMapper;
using MediatR;
using ProjectPierre.Common.Exceptions.Exceptions;

namespace Diiage.QuestService.Application.Handlers;

public class GetQuestByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetQuestByIdQuery, Quest?>
{
    private readonly IGenericRepository<QuestDao> _questRepository = unitOfWork.GetRepository<QuestDao>();
    
    public async Task<Quest?> Handle(GetQuestByIdQuery request, CancellationToken cancellationToken)
    {
        var quest = await _questRepository.GetFirstOrDefaultAsync(predicate: e => e.Id == request.Id ,cancellationToken: cancellationToken);

        return quest == null ? throw new NotFoundException() : mapper.Map<QuestDao, Quest>(quest);
    }
}

