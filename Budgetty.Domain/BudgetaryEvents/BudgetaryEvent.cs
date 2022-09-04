﻿namespace Budgetty.Domain.BudgetaryEvents
{
    public abstract class BudgetaryEvent
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int SequenceNumber { get; set; }
        
        protected abstract string DebugString();

        public override string ToString()
        {
            return DebugString();
        }
    }
}
