import React from "react";
import type { Tag, TagSettings } from "api/types.gen";
import { Input } from "@andrewmclachlan/moo-ds";

export const TagSettingsPanel: React.FC<TagSettingsPanelProps> = ({ tag, onChange }) => {

    if (!tag?.settings) return null;

    const settingChanged = (settings: TagSettings) => {
        onChange?.(settings);
    }

    return (
        <>
            {tag.settings.applySmoothing &&
                <Input.Switch label="Smooth results" onChange={(e) => settingChanged({ ...tag.settings, applySmoothing: e.currentTarget.checked })} />
            }
        </>
    );
}

export interface TagSettingsPanelProps {
    tag?: Tag;
    onChange?: (settings: TagSettings) => void;
}
