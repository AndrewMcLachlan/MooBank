import { Page } from "@andrewmclachlan/mooapp";
import { format, parseISO } from "date-fns";
import { Bill } from "models/bills";
import { Modal, Offcanvas } from "react-bootstrap";

export const BillDetails: React.FC<BillDetailsProps> = ({ bill, onChange, show, onHide }) => {

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
                        <div className="value">{bill.accountName}</div>
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

    return (
        <Modal show={show} onHide={onHide} size="xl" className="transaction-details">
            <Modal.Header closeButton>
                <Modal.Title>Bill</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <section className="transaction-info">
                    <section>
                        <div>Account</div>
                        <div className="value">{bill.accountName}</div>
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
            </Modal.Body>
        </Modal>
    );

}

export interface BillDetailsProps {
    show: boolean;
    onHide: () => void;
    bill: Bill;
    onChange: (bill: Bill) => void;
}
