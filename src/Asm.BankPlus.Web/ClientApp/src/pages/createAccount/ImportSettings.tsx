import React, { useState } from "react";

import { Form } from "react-bootstrap";
import { useImporterTypes } from "../../services";

export const ImportSettings: React.FC<ImportSettingsProps> = (props) => {

    const importerTypes = useImporterTypes();

    const [selectedId, setSelectedId] = useState(0);

    return (
        <Form.Group controlId="importer-type" hidden={!props.show}>
            <Form.Label>Importer Type</Form.Label>
            <Form.Control as="select" value={selectedId.toString()} required onChange={(e) => { setSelectedId(parseInt(e.currentTarget.value)); props.onChange && props.onChange(parseInt(e.currentTarget.value)) }}>
                <option value="0">Select...</option>
                {importerTypes.data?.map(a =>
                    <option value={a.id} key={a.id}>{a.type}</option>
                )}
            </Form.Control>
        </Form.Group>
    );
}

ImportSettings.displayName = "AccountControllerSettings";

export interface ImportSettingsProps {
    show: boolean;
    onChange?: (id: number) => void;
    selectedId: number;
}