using Diiage.QuestService.Application.Requests.Queries;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Models;
using Diiage.QuestService.Repositories.Interfaces;
using MapsterMapper;
using MediatR;

namespace Diiage.QuestService.Application.Handlers;

public class GetPlayerQuestsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetPlayerQuestsQuery, IEnumerable<PlayerQuest>>
{
    private readonly IGenericRepository<PlayerQuestDao> _playerQuestRepository = unitOfWork.GetRepository<PlayerQuestDao>();
    
    public async Task<IEnumerable<PlayerQuest>> Handle(GetPlayerQuestsQuery request, CancellationToken cancellationToken)
    {
        var quests = await _playerQuestRepository.GetMultipleAsync(cancellationToken: cancellationToken);

        return mapper.Map<IEnumerable<PlayerQuestDao>, IEnumerable<PlayerQuest>>(quests);
    }
}