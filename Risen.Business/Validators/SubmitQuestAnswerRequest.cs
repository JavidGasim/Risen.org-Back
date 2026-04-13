using FluentValidation;
using Risen.Contracts.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Validators
{
    public class SubmitQuestAnswerRequestValidator : AbstractValidator<SubmitQuestAnswerRequest>
    {
        public SubmitQuestAnswerRequestValidator()
        {
            RuleFor(x => x.QuestId)
                .NotEmpty().WithMessage("QuestId boş ola bilməz.");

            RuleFor(x => x.SelectedIndex)
                .InclusiveBetween(0, 4).WithMessage("SelectedIndex 0 ilə 4 arasında olmalıdır.");
        }
    }
}
