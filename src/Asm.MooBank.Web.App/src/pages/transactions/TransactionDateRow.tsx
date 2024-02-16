export const TransactionDateRow: React.FC<TransactionDateRowProps> = (props) => {
    return (
        <tr className="date-row">
            <td colSpan={props.colspan}>{props.date}</td>
        </tr>
    );
}

export interface TransactionDateRowProps {
    date: string;
    colspan: number;
}
