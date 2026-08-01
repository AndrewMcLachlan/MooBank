import { Button, Section } from "@andrewmclachlan/moo-ds";
import { useCreateRetirementPlan } from "../-retirement-hooks/useCreateRetirementPlan";
import { emptyPlan } from "../-retirement-utils/retirementDefaults";

interface CreateRetirementPlanProps {
    onPlanCreated: (planId: string) => void;
}

export const CreateRetirementPlan: React.FC<CreateRetirementPlanProps> = ({ onPlanCreated }) => {

    const { createAsync, isPending } = useCreateRetirementPlan();

    const create = async () => {
        const plan = await createAsync(emptyPlan());
        if (plan?.id) onPlanCreated(plan.id);
    };

    return (
        <Section header="Retirement Planner">
            <p>
                Project your superannuation forward to retirement. The plan starts with standard Australian
                assumptions and reads your current balances each time it runs, so it stays up to date on its own.
            </p>
            <p>
                Once it exists you can add yourself and your spouse, choose which superannuation accounts belong
                to each of you, and adjust the return, inflation and contribution assumptions.
            </p>
            <Button variant="primary" onClick={create} disabled={isPending}>Create a Retirement Plan</Button>
        </Section>
    );
};
