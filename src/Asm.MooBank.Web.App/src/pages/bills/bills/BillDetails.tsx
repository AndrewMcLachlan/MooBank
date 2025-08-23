import { format, parseISO } from "date-fns";
import { Bill, BillAccount } from "models/bills";
import { Modal, Offcanvas } from "react-bootstrap";

export const BillDetails: React.FC<BillDetailsProps> = ({ account, bill, onChange, show, onHide }) => {

    if (!bill) return null;

    return (
        <Offcanvas show={show} onHide={onHide} placement="end" className="transaction-details">
            <Offcanvas.Header closeButton>
                <Offcanvas.Title>Bill</Offcanvas.Title>
            </Offcanvas.Header>
            <Offcanvas.Body>
                <section className="transaction-info">
                    <section>
                        <div>Account</div>
                        <div className="value">{account.name}</div>
                        <div>Issue Date</div>
                        <div className="value">{bill.issueDate}</div>
                        <div>Cost</div>
                        <div className="value">{bill.cost}</div>
                    </section>
                </section>
                {bill.periods?.map(period =>
                    <section className="period">
                        <section>
                            <div>Start</div>
                            <div className="value">{format(parseISO(period.periodStart), "dd/MM/yy")}</div>
                            <div>End</div>
                            <div className="value">{format(parseISO(period.periodEnd), "dd/MM/yy")}</div>
                            <div>Service Charge</div>
                            <div className="value">{period.chargePerDay}</div>
                        </section>

                    </section>
                )}
            </Offcanvas.Body>
        </Offcanvas>
    );
}

export interface BillDetailsProps {
    account: BillAccount;
    show: boolean;
    onHide: () => void;
    bill: Bill;
    onChange: (bill: Bill) => void;
}
