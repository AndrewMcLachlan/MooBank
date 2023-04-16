import { ElementType, useRef, useState } from "react";
import { useClickAway } from "@andrewmclachlan/mooapp";
import Select, { ActionMeta, MultiValue } from "react-select";
import Creatable, { CreatableProps } from "react-select/creatable";
import classNames from "classnames";

export const TagPanel = <T extends unknown>(props: TagPanelProps<T, any>) => {

    const [editMode, setEditMode] = useState(false);
    const ref = useRef(null);
    useClickAway(setEditMode, ref);

    const Component = props.allowCreate ? Creatable : Select;

    const extraProps: Partial<CreatableProps<any, any, any>> = props.allowCreate ? {
        onCreateOption: props.onCreate,
        formatCreateLabel: (input) => `Create ${input}...`,
    } : {};

    const onChange = (value: MultiValue<T>, meta: ActionMeta<T>) => {

        switch (meta.action) {
            case "remove-value":
            case "pop-value":
            case "deselect-option":
                props.onRemove && props.onRemove(meta.removedValue);
                break;
            case "clear":
                for (const val of meta.removedValues) {
                    props.onRemove && props.onRemove(val);
                }
                break;
            case "select-option":
                props.onAdd && props.onAdd(meta.option);
        }

        props.onChange && props.onChange(Array.from(value));
    }

    const keyUp: React.KeyboardEventHandler<any> = (e) => {
        if (e.key === "Enter") {
            setEditMode(false);
        }
    }

const getOptionLabel = (item: T) =>  props.labelField(item) ?? (props.allowCreate && "Create new tag...");

    const displayEdit = editMode || props.alwaysShowEditPanel;
    const readonly = !editMode && !props.alwaysShowEditPanel;

    const { as: As, allowCreate,
        alwaysShowEditPanel,
        selectedItems,
        items,
        labelField,
        valueField,
        search,
        onAdd,
        onRemove,
        onCreate, ...rest } = props;

    return (
        <props.as ref={ref} className={classNames("tag-panel", displayEdit && "edit-mode")} onClick={() => setEditMode(true)} onKeyUp={rest.onKeyUp ?? keyUp}>
            <Component unstyled={readonly} {...extraProps} options={props.items} isMulti isClearable value={props.selectedItems} getOptionLabel={getOptionLabel} getOptionValue={props.valueField} onChange={onChange} className={classNames("react-select", readonly && "readonly")} classNamePrefix="react-select" />
        </props.as>
    );
}

TagPanel.displayName = "TagPanel";

TagPanel.defaultProps = {
    as: "div",
    allowCreate: false,
    readonly: false,
    alwaysShowEditPanel: false,
}

export type TagPanelProps<TData, TElement extends ElementType> = Props<TData, TElement> & Omit<React.ComponentPropsWithoutRef<TElement>, keyof Props<TData, TElement>>;

export interface Props<TData, TElement extends ElementType> {

    as?: TElement;
    allowCreate?: boolean;
    alwaysShowEditPanel?: boolean;

    selectedItems: TData[];
    items: TData[];
    labelField: (item: TData) => string;
    valueField: (item: TData) => string;

    onAdd?: (item: TData) => void;
    onRemove?: (item: TData) => void;
    onCreate?: (text: string) => void;
}