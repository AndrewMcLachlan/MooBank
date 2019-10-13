import "./Upload.scss";

import React, { useState } from "react";

export const Upload: React.FC<UploadProps> = (props) => {

    const dragEvents = useDragEvents(props);

    return (
        <div className="upload" onDragEnter={dragEvents.dragEnter} onDragLeave={dragEvents.dragLeave} onDragOver={dragEvents.dragOver} onDrop={dragEvents.drop} >
            <i className="fa fa-upload" />
            <label>
            <input type="file" accept="text/csv" multiple={props.allowMultiple} />
            </label>
        </div>
    );
}

Upload.displayName = "Upload";
Upload.defaultProps = {
    allowMultiple: false,
}

const useDragEvents = (props: UploadProps) => {

    const [isDragging, setIsDragging] = useState(false);
    const [files, setFiles] = useState([] as File[]);

    const stopEvent = (e: React.SyntheticEvent<any>) => {
        e.preventDefault();
        e.stopPropagation();
    }

    const onFilesAdded = (currentFiles: File[], newFiles: File[]) => {
        props.onFilesAdded && props.onFilesAdded({ currentFiles, newFiles });
    }

    const drop = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);

        setIsDragging(false);

        if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {

            const currentFiles = files.concat();
            const newFiles = [];

            if (props.allowMultiple) {

                for (let i = 0; i < e.dataTransfer.files.length;i++) {
                    newFiles.push(e.dataTransfer.files[i]);
                }

                onFilesAdded(currentFiles, newFiles);
                setFiles(files.concat(newFiles));
            }
            else {
                onFilesAdded(currentFiles, [e.dataTransfer.files[0]]);
                setFiles([e.dataTransfer.files[0]]);
            }

            e.dataTransfer.clearData();
        }
    };

    const dragEnter = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);

        if (e.dataTransfer.items && e.dataTransfer.items.length > 0) {
            setIsDragging(true);
        }
    };

    const dragLeave = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);

        setIsDragging(false);
    };

    const dragOver = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);
    };

    return {
        drop,
        dragEnter,
        dragLeave,
        dragOver,
        isDragging,
    };
}

export interface UploadProps {
    allowMultiple?: boolean;
    onFilesAdded?: (e: FilesAddedEvent) => void;
}

export interface FilesAddedEvent {
    currentFiles: File[];
    newFiles: File[];
}