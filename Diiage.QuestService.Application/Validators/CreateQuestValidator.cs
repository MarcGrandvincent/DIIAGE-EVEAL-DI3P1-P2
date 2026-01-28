using Diiage.QuestService.Application.Requests.Commands;
using Diiage.QuestService.Domain.Entities;
using Diiage.QuestService.Repositories.Interfaces;
using FluentValidation;

namespace Diiage.QuestService.Application.Validators;

public class CreateQuestValidator : AbstractValidator<CreateQuestCommand>
{
    public CreateQuestValidator(IUnitOfWork unitOfWork)
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
                                i => i.Code == bl.Code, cancellationToken: cancellationToken);
                    })
                    .WithMessage("Property already exists");
            });
    }
}