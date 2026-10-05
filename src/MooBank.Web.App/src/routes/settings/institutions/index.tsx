import { createFileRoute } from "@tanstack/react-router";
import { changeSortDirection, getNumberOfPages, PageSize, Pagination, PaginationControls, SortablePaginationTh, Section, SectionTable, SortableTh, Badge } from "@andrewmclachlan/moo-ds";
import type { BadgeHue } from "@andrewmclachlan/moo-ds";
import { institutionTypeOptions } from "models/institutions";
import { useNavigate } from "@tanstack/react-router";
import { useGridPageSize, useGridSearch, usePageInRange, useSearchTerm } from "hooks/useGridSearch";
import { useInstitutions } from "hooks/useInstitutions";
import { validateGridSearch, type GridSearchDefaults } from "utils/gridSearch";
import { SettingsPage } from "../-components/SettingsPage";
import { useMemo } from "react";

const institutionsGridDefaults: GridSearchDefaults = { sortField: "name", sortDirection: "Ascending", sortFields: ["name", "type"] };

export const Route = createFileRoute("/settings/institutions/")({
    validateSearch: (search: Record<string, unknown>) => validateGridSearch(search, institutionsGridDefaults),
    component: Institutions,
});

type displayInstitution = {
    id: number;
    name: string;
    type: string;
    typeLabel: string;
}

const hueByType: Record<string, BadgeHue> = {
    Bank:  "blue",
    SuperannuationFund:  "indigo",
    Broker: "amber",
    CreditUnion: "orange",
    BuildingSociety:"rose",
    InvestmentFund: "teal",
    Government: "pink",
};

function Institutions() {

    const { data: institutions } = useInstitutions();

    const navigate = useNavigate();

    const { search: { search = "" }, page: pageNumber, sortField, sortDirection, setPage, setSort, setFilter } = useGridSearch(institutionsGridDefaults);
    const [pageSize, setPageSize] = useGridPageSize("institutions-page-size", 50);
    const [searchTerm, setSearchTerm] = useSearchTerm(search, (term) => setFilter({ search: term || undefined }));

    const matchingInstitutions = useMemo(() => {
        const term = search.toLocaleLowerCase();
        const matching = (term === "" ? institutions : institutions?.filter(i => i?.name.toLocaleLowerCase().includes(term))) ?? [];

        return matching.map((i): displayInstitution => ({
            id: i.id,
            name: i.name,
            type: i.institutionType,
            typeLabel: institutionTypeOptions.find(t => t.value === i.institutionType)?.label,
        }));
    }, [institutions, search]);

    const pagedInstitutions = useMemo(() => {
        const field = sortField as keyof displayInstitution;
        const sorted = [...matchingInstitutions].sort((a, b) => sortDirection === "Ascending"
            ? String(a[field]).localeCompare(String(b[field]))
            : String(b[field]).localeCompare(String(a[field])));

        return sorted.slice((pageNumber - 1) * pageSize, pageNumber * pageSize);
    }, [matchingInstitutions, sortField, sortDirection, pageNumber, pageSize]);

    const numberOfPages = getNumberOfPages(matchingInstitutions.length, pageSize);

    usePageInRange(pageNumber, numberOfPages, !!institutions);

    const sort = (field: string) => setSort(field, changeSortDirection(sortDirection));

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }]} actions={[{ id: "add", label: "Add Institution", icon: "plus", variant: "primary", group: "write", to: "/settings/institutions/add" }]}>
            <Section>
                <input className="form-control" type="text" placeholder="Search" value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} />
            </Section>
            <SectionTable striped hover>
                <thead>
                    <tr>
                        <SortableTh field="name" onSort={sort} sortField={sortField} sortDirection={sortDirection}>Name</SortableTh>
                        <SortablePaginationTh
                            field="type" sortField={sortField} sortDirection={sortDirection} onSort={sort}
                            pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_, newPage) => setPage(newPage)}>
                            Type
                        </SortablePaginationTh>
                    </tr>
                </thead>
                <tbody>
                    {pagedInstitutions.map((f) => (
                        <tr key={f.id} className="clickable" onClick={() => navigate({ to: `/settings/institutions/${f.id}` })}>
                            <td>{f.name}</td>
                            <td><Badge pill muted bg={hueByType[f.type]}>{f.typeLabel}</Badge></td>
                        </tr>
                    ))}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={2}>
                            <PaginationControls>
                                <PageSize value={pageSize} onChange={setPageSize} />
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_, newPage) => setPage(newPage)} />
                            </PaginationControls>
                        </td>
                    </tr>
                </tfoot>
            </SectionTable>
        </SettingsPage>
    );
}
