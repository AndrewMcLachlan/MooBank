import React, { createContext, useContext, ReactNode } from "react";
import { useLocalStorage } from "@andrewmclachlan/mooapp";

interface TransactionListContextType {
  showNet: boolean;
  setShowNet: (value: boolean) => void;
}

const TransactionListContext = createContext<TransactionListContextType | undefined>(undefined);

interface TransactionListProviderProps {
  children: ReactNode;
}

export const TransactionListProvider: React.FC<TransactionListProviderProps> = ({ children }) => {
  const [showNet, setShowNet] = useLocalStorage<boolean>("show-net", false);

  const value: TransactionListContextType = {
    showNet,
    setShowNet,
  };

  return (
    <TransactionListContext.Provider value={value}>
      {children}
    </TransactionListContext.Provider>
  );
};

export const useTransactionList = (): TransactionListContextType => {
  const context = useContext(TransactionListContext);
  if (context === undefined) {
    throw new Error("useTransactionList must be used within a TransactionListProvider");
  }
  return context;
};
