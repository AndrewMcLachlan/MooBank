import { createFileRoute } from "@tanstack/react-router";
import { Button, Form, Section, SectionTable } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { toast } from "react-toastify";
import type { GrowthStrategyRates } from "api/types.gen";
import { SettingsPage } from "../-components/SettingsPage";
import { useGrowthStrategyRates, useSaveGrowthStrategyRate } from "./-hooks/useGrowthStrategyRates";

export const Route = createFileRoute("/settings/returns/")({
    component: ReturnSettings,
});

/** The form holds each rate as a percentage, which is how a return is talked about. */
type ReturnRatesFormValues = Record<string, number>;

const toPercent = (rate?: number | null) => Math.round((rate ?? 0) * 10_000) / 100;

/** Custom has no rate to edit: it means whatever figure a member chose for themselves. */
const editable = (rates?: GrowthStrategyRates[]) => (rates ?? []).filter(r => r.strategy !== "Custom");

const toFormValues = (rates?: GrowthStrategyRates[]): ReturnRatesFormValues =>
    Object.fromEntries(editable(rates).map(r => [r.strategy, toPercent(r.rate)]));

function ReturnSettings() {

    const { data: rates } = useGrowthStrategyRates();
    const { mutateAsync, isPending } = useSaveGrowthStrategyRate();

    const form = useForm<ReturnRatesFormValues>({
        values: toFormValues(rates),
        resetOptions: { keepDirtyValues: true },
    });

    const handleSave = async (data: ReturnRatesFormValues) => {
        // One row per strategy on the server, so a changed rate is saved against its own strategy
        // rather than the set being replaced wholesale.
        const changed = editable(rates).filter(r => toPercent(r.rate) !== Number(data[r.strategy]));

        if (changed.length === 0) return;

        await toast.promise(
            Promise.all(changed.map(r => mutateAsync({
                body: {
                    rate: {
                        strategy: r.strategy,
                        description: r.description,
                        rate: (Number(data[r.strategy]) || 0) / 100,
                    },
                },
            }))),
            { pending: "Saving return rates", success: "Return rates saved", error: "Could not save the return rates" },
        );
    };

    return (
        <SettingsPage title="Return Assumptions" breadcrumbs={[{ text: "Return Assumptions", route: "/settings/returns" }]}>
            <Section header="About these figures">
                <p className="pension-note">
                    These are assumptions about markets rather than about any one household, so they apply to every
                    retirement plan and a change here moves them all together. A member set to <strong>Custom</strong> is
                    the exception: their rate belongs to them and is unaffected by anything on this page.
                </p>
                <p className="pension-note">
                    The seeded figures are ASIC&rsquo;s, as published for the MoneySmart calculators. They are
                    <strong> net of investment fees and of the tax on fund earnings</strong>; administration fees and
                    insurance premiums are charged separately, against each member. Replacing one with a fund&rsquo;s
                    headline gross return would count both twice and make every projection read high.
                </p>
            </Section>

            <Section header="Assumed Return">
                <Form form={form} onSubmit={handleSave}>
                    <div className="pension-fields">
                        {editable(rates).map(r => (
                            <Form.Group key={r.strategy} groupId={r.strategy}>
                                <Form.Label>{r.description} (% a year)</Form.Label>
                                <Form.Input type="number" step="0.1" />
                            </Form.Group>
                        ))}
                    </div>
                    <div className="pension-actions">
                        <Button type="submit" variant="primary" disabled={isPending}>
                            {isPending ? "Saving…" : "Save"}
                        </Button>
                    </div>
                </Form>
            </Section>

            <SectionTable header="In Use">
                <thead>
                    <tr>
                        <th>Strategy</th>
                        <th>Assumed Return</th>
                    </tr>
                </thead>
                <tbody>
                    {rates?.map(r => (
                        <tr key={r.strategy}>
                            <td>{r.description}</td>
                            <td>{r.rate == null ? "set on the member" : `${toPercent(r.rate)}%`}</td>
                        </tr>
                    ))}
                </tbody>
            </SectionTable>
        </SettingsPage>
    );
}
