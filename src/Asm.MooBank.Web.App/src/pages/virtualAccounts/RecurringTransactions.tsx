import { EditColumn, SectionTable } from "@andrewmclachlan/moo-ds";
import { DeleteIcon } from "@andrewmclachlan/moo-ds";
import { SaveIcon } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import { parse } from "date-fns/parse";
import { parseISO } from "date-fns/parseISO";
import { VirtualInstrument } from "models";
import { RecurringTransaction, Schedule, Schedules, emptyRecurringTransaction } from "models/RecurringTransaction";
import React, { useState } from "react";
import { useCreateRecurringTransaction, useDeleteRecurringTransaction, useGetRecurringTransactions, useUpdateRecurringTransaction } from "services/RecurringTransactionService";

export const RecurringTransactions: React.FC<RecurringTransactionsProps> = ({ account }) => {

    const {data: recurringTransactions } = useGetRecurringTransactions(account.parentId, account.id);

    const accountId = account.parentId;
    const virtualId = account.id;

    const createRecurringTransaction = useCreateRecurringTransaction(accountId, virtualId);
    const updateRecurringTransaction = useUpdateRecurringTransaction(accountId, virtualId);
    const deleteRecurringTransaction = useDeleteRecurringTransaction(accountId, virtualId);

    const create = () => {
        createRecurringTransaction(newRT);
        setNewRT(emptyRecurringTransaction(virtualId));
    }

    const [newRT, setNewRT] = useState<RecurringTransaction>(emptyRecurringTransaction(virtualId));

    const onDelete = (id: string) => {
        if (confirm("Are you sure you want to delete this recurring transaction?")) {
            deleteRecurringTransaction(id);
        }
    }

    return (
        <SectionTable striped hover header="Recurring Transactions">
            <thead>
                <tr>
                    <th>Description</th>
                    <th>Amount</th>
                    <th>Schedule</th>
                    <th>Last Run</th>
                    <th>Next Run</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td><input type="text" className="form-control" placeholder="Description" value={newRT.description} onChange={e => setNewRT({ ...newRT, description: e.currentTarget.value })} /></td>
                    <td><input type="number" className="form-control" placeholder="Amount" value={newRT.amount} onChange={e => setNewRT({ ...newRT, amount: e.currentTarget.valueAsNumber })} /></td>
                    <td>
                        <select className="form-control" value={newRT.schedule} onChange={e => setNewRT({ ...newRT, schedule: e.currentTarget.value as Schedule })}>
                            {Schedules.map(s => <option key={s}>{s}</option>)}
                        </select>
                    </td>
                    <td>-</td>
                    <td><input type="date" className="form-control" placeholder="Next Run" value={newRT.nextRun} onChange={e => setNewRT({ ...newRT, nextRun: e.currentTarget.value })} /></td>
                    <td className="row-action"><SaveIcon onClick={create} /></td>
                </tr>
                {recurringTransactions && recurringTransactions.map(a => (
                    <tr key={a.id}>
                        <EditColumn value={a.description} onChange={target => updateRecurringTransaction({ ...a, description: target.value })} />
                        <EditColumn type="number" value={a.amount.toString()} onChange={value => updateRecurringTransaction({ ...a, amount: Number(value) })} />
                        <td>{a.schedule}</td>
                        <td>{a.lastRun && format(parseISO(a.lastRun), "dd/MM/yyyy HH:mm")}</td>
                        <EditColumn value={a.nextRun} onChange={target => updateRecurringTransaction({ ...a, nextRun: target.value })} type="date">
                            {format(parse(a.nextRun, "yyyy-MM-dd", new Date()), "dd/MM/yyyy")}
                        </EditColumn>
                        <td className="row-action"><DeleteIcon onClick={() => onDelete(a.id)} /></td>
                    </tr>
                ))}
            </tbody>
        </SectionTable>
    );
}

export interface RecurringTransactionsProps {
    account: VirtualInstrument;
}
