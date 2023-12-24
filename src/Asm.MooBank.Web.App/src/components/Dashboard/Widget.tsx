import { Section } from "@andrewmclachlan/mooapp";
import { PropsWithChildren } from "react";
import { Col, Spinner } from "react-bootstrap";

export const Widget: React.FC<PropsWithChildren<WidgetProps>> = ({ children, loading, ...rest }) => (
    <Col xl={4} lg={6} md={12}>
        <Section {...rest}>
        {loading && <Spinner />}
        {!loading &&children}
        </Section>
    </Col>
)

Widget.defaultProps = {
    loading: false,
}

export interface WidgetProps
{
    loading?: boolean;
    title?: string;
    size?: 1 | 2 | 3 | 4 | 5 | 6;
    className?: string;
}