using System.Linq.Expressions;
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
        Expression<Func<QuestDao, bool>>? predicate = null;
        
        if (request.IsActive.HasValue || !string.IsNullOrWhiteSpace(request.Query))
        {
            predicate = q =>
                (!request.IsActive.HasValue || q.IsActive == request.IsActive.Value) &&
                (string.IsNullOrWhiteSpace(request.Query) || 
                 q.Code.Contains(request.Query) || 
                 q.Title.Contains(request.Query) || 
                 q.Description.Contains(request.Query));
        }

        var quests = await _questRepository.GetMultipleAsync(
            predicate: predicate,
            cancellationToken: cancellationToken);

        return mapper.Map<IEnumerable<QuestDao>, IEnumerable<Quest>>(quests);
    }
}