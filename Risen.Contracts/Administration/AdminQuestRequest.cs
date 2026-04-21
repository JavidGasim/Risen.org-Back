using Risen.Entities.Entities;
using System;
using System.Collections.Generic;

namespace Risen.Contracts.Administration
{
    public sealed record AdminQuestRequest(
        string QuestionText,
        IEnumerable<string> Options,
        int CorrectOptionIndex,
        QuestDifficulty Difficulty,
        string? SubjectCode,
        int BaseXp = 10,
        bool IsPremiumOnly = false,
        string? Description = null
    );
}
