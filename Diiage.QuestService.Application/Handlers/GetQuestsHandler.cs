using Diiage.QuestService.Application.Requests.Queries;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Domain.Models;
using Diiage.QuestService.Repositories.Interfaces;
using MapsterMapper;
using MediatR;

namespace Diiage.QuestService.Application.Handlers;

public class GetQuestsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetQuestsQuery, IEnumerable<Quest>>
{
    private readonly IGenericRepository<QuestDao> _questRepository = unitOfWork.GetRepository<QuestDao>();
    
    public async Task<IEnumerable<Quest>> Handle(GetQuestsQuery request, CancellationToken cancellationToken)
    {
        var quests = await _questRepository.GetMultipleAsync(
            predicate: request.IsActive.HasValue ? q => q.IsActive == request.IsActive.Value : null,
            cancellationToken: cancellationToken);

        return mapper.Map<IEnumerable<QuestDao>, IEnumerable<Quest>>(quests);
    }
}