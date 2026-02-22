import { createFileRoute } from "@tanstack/react-router";
import { changeSortDirection, IconLinkButton, Input, MiniPagination, PageSize, Pagination, PaginationControls, SortablePaginationTh, Section, SectionTable, SortableTh, SortDirection, useLocalStorage } from "@andrewmclachlan/moo-ds";
import { institutionTypeOptions } from "helpers/institutions";
import { useNavigate } from "@tanstack/react-router";
import { useInstitutions } from "hooks/useInstitutions";
import { SettingsPage } from "../-components/SettingsPage";
import { use, useEffect, useState } from "react";

export const Route = createFileRoute("/settings/institutions/")({
    component: Institutions,
});

type displayInstitution = {
    id: number;
    name: string;
    type: string;
}

function Institutions() {

    const { data: institutions } = useInstitutions();

    const navigate = useNavigate();

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, setPageSize] = useLocalStorage<number>("institutions-page-size", 50);
    const [sortDirection, setSortDirection] = useState<SortDirection>("Ascending");
    const [search, setSearch] = useState("");
    const [sortField, setSortField] = useState<keyof displayInstitution>("name");

    const [filteredInstitutions, setFilteredInstitutions] = useState<any[]>([]);

    const numberOfPages = Math.ceil((institutions?.length ?? 0) / pageSize);

    useEffect(() => {
        const searchTerm = search.toLocaleLowerCase();
        const matchingInstitutions = (searchTerm === "" ? institutions : institutions?.filter(i => i?.name.toLocaleLowerCase().includes(searchTerm))) ?? [];

        const transformed = matchingInstitutions.map((i) => {
            return {
                id: i.id,
                name: i.name,
                type: institutionTypeOptions.find(t => t.value === i.institutionType)?.label
            } as displayInstitution
        });

        transformed.sort((a, b) => {
            if (sortDirection === "Ascending") {
                return (a[sortField] as string).localeCompare(b[sortField] as string);
            }
            return (b[sortField] as string).localeCompare(a[sortField] as string);
        });

        const pagedInstitutions = transformed.slice((pageNumber - 1) * pageSize, ((pageNumber - 1) * pageSize) + pageSize);

        setFilteredInstitutions(pagedInstitutions);
    }, [JSON.stringify(institutions), search, pageSize, pageNumber, sortDirection]);

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }]} actions={[<IconLinkButton variant="primary" key="add" to="/settings/institutions/add" icon="plus">Add Institution</IconLinkButton>]}>
            <Section>
                <input className="form-control" type="text" placeholder="Search" value={search} onChange={(e) => setSearch(e.target.value)} />
            </Section>
            <SectionTable striped hover>
                <thead>
                    <tr>
                        <SortableTh field="name" onSort={(field) => { setSortField(field as keyof displayInstitution); setSortDirection(changeSortDirection(sortDirection)) }} sortField={sortField} sortDirection={sortDirection}>Name</SortableTh>
                        <SortablePaginationTh
                            field="type" sortField={sortField} sortDirection={sortDirection} onSort={(field) => { setSortField(field as keyof displayInstitution); setSortDirection(changeSortDirection(sortDirection)) }}
                            pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_, newPage) => setPageNumber(newPage)}>
                            Type
                        </SortablePaginationTh>
                    </tr>
                </thead>
                <tbody>
                    {filteredInstitutions && filteredInstitutions.map((f) => (
                        <tr key={f.id} className="clickable" onClick={() => navigate({ to: `/settings/institutions/${f.id}` })}>
                            <td>{f.name}</td>
                            <td>{f.type}</td>
                        </tr>
                    ))}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={2}>
                            <PaginationControls>
                                <PageSize value={pageSize} onChange={setPageSize} />
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_, newPage) => setPageNumber(newPage)} />
                            </PaginationControls>
                        </td>
                    </tr>
                </tfoot>
            </SectionTable>
        </SettingsPage>
    );
}
