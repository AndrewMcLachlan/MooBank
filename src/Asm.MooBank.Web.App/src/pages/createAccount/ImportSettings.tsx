import React, { useEffect, useState } from "react";

import { Form } from "react-bootstrap";
import { useImporterTypes } from "services";

export const ImportSettings: React.FC<ImportSettingsProps> = (props) => {

    const importerTypes = useImporterTypes();

    const [selectedId, setSelectedId] = useState(0);

    useEffect(() => {
        setSelectedId(props.selectedId ?? 0);
    }, [props.selectedId]);

    return (
        <Form.Group controlId="importer-type" hidden={!props.show}>
            <Form.Label>Importer Type</Form.Label>
            <Form.Select value={selectedId.toString()} required onChange={(e) => { setSelectedId(parseInt(e.currentTarget.value)); props.onChange?.(parseInt(e.currentTarget.value)) }}>
                <option value="0">Select...</option>
                {importerTypes.data?.map(a =>
                    <option value={a.id} key={a.id}>{a.name}</option>
                )}
            </Form.Select>
        </Form.Group>
    );
}

ImportSettings.displayName = "AccountControllerSettings";

export interface ImportSettingsProps {
    show: boolean;
    onChange?: (id: number) => void;
    selectedId?: number;
}
