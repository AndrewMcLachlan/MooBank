import React from "react";
import { Tag, TagSettings } from "models";
import { Form } from "react-bootstrap";

export const TagSettingsPanel:React.FC<TagSettingsPanelProps> = ({tag, onChange}) => {

    if (!tag?.settings) return null;

    const settingChanged = (settings: TagSettings) => {
        onChange && onChange(settings);
    }

    return (
        <section className="tag-settings">
            {tag.settings.applySmoothing &&
                <Form.Check label="Smooth results" onChange={(e) => settingChanged({...tag.settings, applySmoothing: e.currentTarget.checked})} />
            }
        </section>
    );
}

export interface TagSettingsPanelProps {
    tag?: Tag;
    onChange?: (settings: TagSettings) => void;
}