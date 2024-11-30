import React, { useEffect, useRef, useState } from "react";

import { useBreakdownReport, useTag } from "services";

import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import { Doughnut, getElementAtEvent } from "react-chartjs-2";

import { Period } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { useNavigate, useParams } from "react-router";
import { chartColours } from "../../../helpers/chartColours";

ChartJS.register(...registerables);

export const Breakdown: React.FC<BreakdownProps> = ({accountId, tagId, period, reportType}) => {

    const navigate = useNavigate();

    const [selectedTagId, setSelectedTagId] = useState<number | undefined>(tagId ? Number(tagId) : undefined);
    const [_previousTagId, setPreviousTagId] = useState<number | undefined>();

    const report = useBreakdownReport(accountId!, period?.startDate, period?.endDate, reportType, selectedTagId);

    const chartRef = useRef();

    useEffect(() => {
        setSelectedTagId(tagId ? Number(tagId) : undefined);
    }, [tagId]);

    const dataset: ChartData<"doughnut", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.netAmount) ?? [],
            backgroundColor: chartColours,
            borderRadius: 10,
            spacing: 10,
            borderColor: "transparent",
        }],
    };

    return (
        <Doughnut id="bytag" className="bob" ref={chartRef} data={dataset} options={{
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "right"
                },
                tooltip: {
                    mode: "point",
                    intersect: false,
                } as any,
            },
            hover: {
                mode: "point",
                intersect: true,
            },
        }}
            onClick={(e) => {
                const elements = getElementAtEvent(chartRef.current!, e);
                if (elements.length !== 1) return;
                const tag = report.data!.tags[elements[0].index];
                if (!tag.hasChildren || tag.tagId === selectedTagId) {

                    const url = !selectedTagId ? `/accounts/${accountId}?untagged=true` : `/accounts/${accountId}?tag=${tag.tagId}`;

                    navigate(url);
                    return;
                }
                setPreviousTagId(selectedTagId);
                setSelectedTagId(tag.tagId);
                navigate(`/accounts/${accountId}/reports/breakdown/${tag.tagId}`);
            }} />
    );
}

export interface BreakdownProps {
    accountId: string;
    tagId?: string;
    period: Period;
    reportType: ReportType;
}
