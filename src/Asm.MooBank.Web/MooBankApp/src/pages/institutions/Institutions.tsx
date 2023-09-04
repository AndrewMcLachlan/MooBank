import { Page } from "@andrewmclachlan/mooapp";
import { Table } from "react-bootstrap";
import { useInstitutions } from "services";

export const Institutions = () => {

    const { data: institutions, isLoading } = useInstitutions();

    return (
        <Page title="Institutions">
            <Table className="section" striped hover>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {institutions && institutions.map((f) => (
                        <tr key={f.id}>
                            <td>{f.name}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </Page>
    )
}