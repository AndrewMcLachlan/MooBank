import React from "react";
import { Tag, TagSettings } from "models";
import { Form } from "react-bootstrap";

export const TagSettingsPanel: React.FC<TagSettingsPanelProps> = ({ tag, onChange }) => {

    if (!tag?.settings) return null;

    const settingChanged = (settings: TagSettings) => {
        onChange?.(settings);
    }

    return (
        <>
            {tag.settings.applySmoothing &&
                <Form.Switch label="Smooth results" onChange={(e) => settingChanged({ ...tag.settings, applySmoothing: e.currentTarget.checked })} />
            }
        </>
    );
}

export interface TagSettingsPanelProps {
    tag?: Tag;
    onChange?: (settings: TagSettings) => void;
}
