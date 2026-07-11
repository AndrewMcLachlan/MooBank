import React from "react";
import type { PropsWithChildren } from "react";

import type { PageProps } from "@andrewmclachlan/moo-app";
import { Sliders } from "@andrewmclachlan/moo-icons";

import { InstrumentPage } from "components";
import { useAsset } from "./AssetProvider";

export const AssetPage: React.FC<PropsWithChildren<AssetPageProps>> = ({ children, ...props }) => {

    const asset = useAsset();

    return (
        <InstrumentPage instrument={asset} instrumentRoute={`/assets/${asset?.id}`} instrumentNavItems={[{ route: `/assets/${asset?.id}/manage`, text: "Manage", image: <Sliders /> }]} {...props}>
            {children}
        </InstrumentPage>
    )
}

export interface AssetPageProps extends PageProps {
}
