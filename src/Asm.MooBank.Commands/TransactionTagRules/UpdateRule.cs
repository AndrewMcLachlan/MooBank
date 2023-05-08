namespace Asm.MooBank.Models.Commands.TransactionTagRules;

public record UpdateRule(Guid AccountId, int RuleId, TransactionTagRule Rule) : ICommand<TransactionTagRule>;
