import { Page } from "@andrewmclachlan/mooapp";
import { Table } from "react-bootstrap";
import { useFamilies } from "services";

export const Families = () => {

    const { data: families, isLoading } = useFamilies();

    return (
        <Page title="Families">
            <Table className="section" striped hover>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {families && families.map((f) => (
                        <tr key={f.id}>
                            <td>{f.name}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </Page>
    )
}