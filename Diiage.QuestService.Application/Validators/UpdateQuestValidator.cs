using Diiage.QuestService.Application.Requests.Commands;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Repositories.Interfaces;
using FluentValidation;

namespace Diiage.QuestService.Application.Validators;

public class UpdateQuestValidator : AbstractValidator<UpdateQuestCommand>
{
    public UpdateQuestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(q => q.Code)
            .NotNull()
            .NotEmpty()
            .DependentRules(() =>
            {
                RuleFor(u => u.Code).MustAsync(async (bl, code, cancellationToken) =>
                    {
                        return !await unitOfWork.GetRepository<QuestDao>()
                            .ExistsAsync(
                                i => i.Code == bl.Code && i.Id != bl.Id, cancellationToken: cancellationToken);
                    })
                    .WithMessage("Property already exists");
            });
    }
}