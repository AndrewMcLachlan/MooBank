import { createFileRoute } from "@tanstack/react-router";
import { Button, Form, Section, SectionTable } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { toast } from "react-toastify";
import type { PensionRates } from "api/types.gen";
import { Amount, CurrencyInput } from "components";
import { formatDisplayDate } from "utils/dateFns";
import { SettingsPage } from "../-components/SettingsPage";
import { usePensionRates, useSavePensionRates } from "./-hooks/usePensionRates";

export const Route = createFileRoute("/settings/pension/")({
    component: PensionSettings,
});

/** The form holds the taper as a percentage, which is how it is talked about. */
interface PensionRatesFormValues {
    effectiveFrom: string;
    eligibilityAge: number;
    maxAnnualSingle: number;
    maxAnnualCouple: number;
    assetsFreeAreaSingle: number;
    assetsFreeAreaCouple: number;
    assetsTaperPercent: number;
}

const toFormValues = (rates?: PensionRates): PensionRatesFormValues => ({
    effectiveFrom: rates?.effectiveFrom ?? "",
    eligibilityAge: rates?.eligibilityAge ?? 67,
    maxAnnualSingle: rates?.maxAnnualSingle ?? 0,
    maxAnnualCouple: rates?.maxAnnualCouple ?? 0,
    assetsFreeAreaSingle: rates?.assetsFreeAreaSingle ?? 0,
    assetsFreeAreaCouple: rates?.assetsFreeAreaCouple ?? 0,
    assetsTaperPercent: Math.round((rates?.assetsTaperRate ?? 0) * 10_000) / 100,
});

function PensionSettings() {

    const { data: rates } = usePensionRates();
    const { mutateAsync, isPending } = useSavePensionRates();

    // Newest first from the server, so the first row is the set in force.
    const current = rates?.[0];

    const form = useForm<PensionRatesFormValues>({
        values: toFormValues(current),
        resetOptions: { keepDirtyValues: true },
    });

    const handleSave = async (data: PensionRatesFormValues) => {
        await toast.promise(
            mutateAsync({
                body: {
                    rates: {
                        // The server keys a set of rates on its effective date, so the id it carries
                        // back is informational — a new date records a new set either way.
                        id: current?.effectiveFrom === data.effectiveFrom ? current.id : 0,
                        effectiveFrom: data.effectiveFrom,
                        eligibilityAge: Number(data.eligibilityAge) || 0,
                        maxAnnualSingle: Number(data.maxAnnualSingle) || 0,
                        maxAnnualCouple: Number(data.maxAnnualCouple) || 0,
                        assetsFreeAreaSingle: Number(data.assetsFreeAreaSingle) || 0,
                        assetsFreeAreaCouple: Number(data.assetsFreeAreaCouple) || 0,
                        assetsTaperRate: (Number(data.assetsTaperPercent) || 0) / 100,
                    },
                },
            }),
            { pending: "Saving pension rates", success: "Pension rates saved", error: "Could not save the pension rates" },
        );
    };

    return (
        <SettingsPage title="Age Pension" breadcrumbs={[{ text: "Age Pension", route: "/settings/pension" }]}>
            <Section header="About these figures">
                <p className="pension-note">
                    These are national figures, so they apply to every retirement plan. Services Australia reindexes them
                    each March and September and publishes no feed to read them from, so they have to be entered by hand —
                    <strong> check them against the current published rates</strong>. The seeded values are approximate and
                    will be out of date.
                </p>
                <p className="pension-note">
                    The asset free areas are the homeowner ones. A household that does not own its home has considerably
                    higher thresholds, which is not modelled separately; enter your own figures instead. Only the assets
                    test is applied — the income test is not modelled, on the basis that for a retiree whose assets are
                    mostly superannuation the assets test is the binding one.
                </p>
            </Section>

            <Section header={current ? `In force from ${formatDisplayDate(current.effectiveFrom)}` : "Enter the current rates"}>
                <Form form={form} onSubmit={handleSave}>
                    <div className="pension-fields">
                        <Form.Group groupId="effectiveFrom">
                            <Form.Label>In Force From</Form.Label>
                            <Form.Input type="date" />
                        </Form.Group>
                        <Form.Group groupId="eligibilityAge">
                            <Form.Label>Eligibility Age</Form.Label>
                            <Form.Input type="number" step="1" />
                        </Form.Group>
                        <Form.Group groupId="maxAnnualSingle">
                            <Form.Label>Maximum a Year — Single</Form.Label>
                            <CurrencyInput currency="AUD" />
                        </Form.Group>
                        <Form.Group groupId="maxAnnualCouple">
                            <Form.Label>Maximum a Year — Couple (combined)</Form.Label>
                            <CurrencyInput currency="AUD" />
                        </Form.Group>
                        <Form.Group groupId="assetsFreeAreaSingle">
                            <Form.Label>Assets Free Area — Single</Form.Label>
                            <CurrencyInput currency="AUD" />
                        </Form.Group>
                        <Form.Group groupId="assetsFreeAreaCouple">
                            <Form.Label>Assets Free Area — Couple (combined)</Form.Label>
                            <CurrencyInput currency="AUD" />
                        </Form.Group>
                        <Form.Group groupId="assetsTaperPercent">
                            <Form.Label>Taper (% of assets over the free area, a year)</Form.Label>
                            <Form.Input type="number" step="0.1" />
                        </Form.Group>
                    </div>
                    <div className="pension-actions">
                        <Button type="submit" variant="primary" disabled={isPending}>
                            {isPending ? "Saving…" : "Save"}
                        </Button>
                    </div>
                </Form>
                <p className="pension-note">
                    Saving against a new date records a new set and leaves the old one as history; saving against a date
                    already recorded corrects it. A projection uses the most recent set on or before the day it runs, so
                    next March's rates can be entered ahead of time without changing today's answers.
                </p>
            </Section>

            {(rates?.length ?? 0) > 1 && (
                <SectionTable header="History">
                    <thead>
                        <tr>
                            <th>In Force From</th>
                            <th>Age</th>
                            <th>Single</th>
                            <th>Couple</th>
                            <th>Free Area (Single)</th>
                            <th>Free Area (Couple)</th>
                            <th>Taper</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rates?.map(r => (
                            <tr key={r.id}>
                                <td>{formatDisplayDate(r.effectiveFrom)}</td>
                                <td>{r.eligibilityAge}</td>
                                <td><Amount amount={r.maxAnnualSingle} currencyCode="AUD" decimalPlaces={0} /></td>
                                <td><Amount amount={r.maxAnnualCouple} currencyCode="AUD" decimalPlaces={0} /></td>
                                <td><Amount amount={r.assetsFreeAreaSingle} currencyCode="AUD" decimalPlaces={0} /></td>
                                <td><Amount amount={r.assetsFreeAreaCouple} currencyCode="AUD" decimalPlaces={0} /></td>
                                <td>{Math.round(r.assetsTaperRate * 10_000) / 100}%</td>
                            </tr>
                        ))}
                    </tbody>
                </SectionTable>
            )}
        </SettingsPage>
    );
}
