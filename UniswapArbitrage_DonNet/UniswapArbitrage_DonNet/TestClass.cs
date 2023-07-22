using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UniswapArbitrage_DonNet
{
    class TestClass
    {

        static async Task uniswap_test()
        {

            var privateKey = "YOUR KEY";
            var account = new Account(privateKey);
            var web3 = new Web3(account, "https://polygon-rpc.com/");
            var gas = await web3.Eth.GasPrice.SendRequestAsync();
            var contract_uniswap_swap = web3.Eth.GetContractHandler("0xe592427a0aece92de3edee1f18e0157c05861564");

            var Params = new ExactInputSingle
            {
                tokenIn = "0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619",
                tokenOut = "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
                fee = 500,
                recipient = account.Address,
                amountIn = new BigInteger(5000341279270),
                deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(10).ToUnixTimeSeconds()),
                amountOutMinimum = 0,
                sqrtPriceLimitX96 = 0
            };
            var swapeParams = new ExactInputSingleFunctionBase { Params = Params };
            swapeParams.GasPrice = gas.Value * 3;
            //swapeParams.Gas = 156897;
            var sushiswap_weth_swape = await contract_uniswap_swap.SendRequestAsync(swapeParams);

        }

        static async Task sushiswap_test()
        {
            var privateKey = "YOUR KEY";
            var account = new Account(privateKey);
            var web3 = new Web3(account, "https://polygon-rpc.com/");
            var gas = await web3.Eth.GasPrice.SendRequestAsync();

            List<string> path = new List<string>();
            path.Add("0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174");
            path.Add("0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619");
            var sushiswapParams = new swapExactTokensForTokensSupportingFeeOnTransferTokensFunction
            {
                AmountIn = new BigInteger(10000),
                AmountOutMin = 0,
                Path = path,
                To = account.Address,
                Deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(15).ToUnixTimeSeconds())
            };

            sushiswapParams.GasPrice = gas.Value * 3;
            //sushiswapParams.Gas = 1000000;

            //swap
            var contract_sushiswap_swap = web3.Eth.GetContractHandler("0x1b02dA8Cb0d097eB8D57A175b88c7D8b47997506");
            var sushiswap_weth_swape = await contract_sushiswap_swap.SendRequestAsync(sushiswapParams);

            var a = 0;
        }

        static async Task swap_test()
        {
            var privateKey = "YOUR KEY";
            var account = new Account(privateKey);
            var web3 = new Web3(account, "https://polygon-rpc.com/");

            var contract_uniswap_swap = web3.Eth.GetContractHandler("0xe592427a0aece92de3edee1f18e0157c05861564");

            var Params = new ExactInputSingle
            {
                tokenIn = "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
                tokenOut = "0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619",
                fee = 100,
                recipient = account.Address,
                amountIn = new BigInteger(10000),
                deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(10).ToUnixTimeSeconds()),
                amountOutMinimum = 0,
                sqrtPriceLimitX96 = 0
            };
            var uniswapeParams = new ExactInputSingleFunctionBase { Params = Params };
            uniswapeParams.GasPrice = Web3.Convert.ToWei(400, UnitConversion.EthUnit.Gwei);
            var contract_sushiswap_swap = web3.Eth.GetContractHandler("0x1b02dA8Cb0d097eB8D57A175b88c7D8b47997506");


            List<string> path = new List<string>();
            path.Add("0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619");
            path.Add("0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174");

            var sushiswapParams = new swapExactTokensForTokensSupportingFeeOnTransferTokensFunction
            {
                AmountIn = new BigInteger(517420954484),
                AmountOutMin = 0,
                Path = path,
                To = account.Address,
                Deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(60).ToUnixTimeSeconds())
            };

            sushiswapParams.GasPrice = Web3.Convert.ToWei(300, UnitConversion.EthUnit.Gwei);


            var uniswap_weth_swape = await contract_uniswap_swap.SendRequestAsync(uniswapeParams);
            var sushiswap_weth_swape = await contract_sushiswap_swap.SendRequestAsync(sushiswapParams);

            var a = "";
        }

        public partial class Slot0Function : Slot0FunctionBase { }

        [Function("slot0", typeof(Slot0OutputDTO))]
        public class Slot0FunctionBase : FunctionMessage
        {

        }

        public partial class Slot0OutputDTO : Slot0OutputDTOBase { }

        [FunctionOutput]
        public class Slot0OutputDTOBase : IFunctionOutputDTO
        {
            [Parameter("uint160", "sqrtPriceX96", 1)]
            public virtual BigInteger SqrtPriceX96 { get; set; }
            [Parameter("int24", "tick", 2)]
            public virtual int Tick { get; set; }
            [Parameter("uint16", "observationIndex", 3)]
            public virtual ushort ObservationIndex { get; set; }
            [Parameter("uint16", "observationCardinality", 4)]
            public virtual ushort ObservationCardinality { get; set; }
            [Parameter("uint16", "observationCardinalityNext", 5)]
            public virtual ushort ObservationCardinalityNext { get; set; }
            [Parameter("uint8", "feeProtocol", 6)]
            public virtual byte FeeProtocol { get; set; }
            [Parameter("bool", "unlocked", 7)]
            public virtual bool Unlocked { get; set; }
        }

        public partial class GetReservesFunction : GetReservesFunctionBase { }

        [Function("getReserves", typeof(Slot0OutputDTO))]
        public class GetReservesFunctionBase : FunctionMessage
        {

        }

        public partial class GetReservesOutputDTO : GetReservesOutputDTOBase { }

        [FunctionOutput]
        public class GetReservesOutputDTOBase : IFunctionOutputDTO
        {
            [Parameter("uint112", "_reserve0", 1)]
            public BigInteger _reserve0 { get; set; }

            [Parameter("uint112", "_reserve1", 2)]
            public BigInteger _reserve1 { get; set; }

            [Parameter("uint32", "_blockTimestampLast", 3)]
            public Int32 _blockTimestampLast { get; set; }
        }


        public partial class ExactInputSingleFunction : ExactInputSingleFunctionBase { }

        [Function("exactInputSingle", "uint256[]")]
        public class ExactInputSingleFunctionBase : FunctionMessage
        {
            [Parameter("tuple", "params", 1)]
            public virtual ExactInputSingle Params { get; set; }

            //[Parameter("address", "params.tokenIn", 1)]
            //public virtual string TokenIn { get; set; }
            //[Parameter("address", "params.tokenOut", 2)]
            //public virtual string TokenOut { get; set; }
            //[Parameter("uint24", "params.fee", 3)]
            //public virtual int Fee { get; set; }
            //[Parameter("address", "params.recipient", 4)]
            //public virtual string Recipient { get; set; }
            //[Parameter("uint256", "params.deadline", 5)]
            //public virtual BigInteger Deadline { get; set; }
            //[Parameter("uint256", "params.amountIn", 6)]
            //public virtual BigInteger AmountIn { get; set; }
            //[Parameter("uint256", "params.amountOutMinimum", 7)]
            //public virtual BigInteger AmountOutMinimum { get; set; }
            //[Parameter("uint160", "params.sqrtPriceLimitX96", 8)]
            //public virtual BigInteger SqrtPriceLimitX96 { get; set; }
        }

        public class ExactInputSingle
        {
            [Parameter("address", "params.tokenIn", 1)]
            public virtual string tokenIn { get; set; }
            [Parameter("address", "params.tokenOut", 2)]
            public virtual string tokenOut { get; set; }
            [Parameter("uint24", "params.fee", 3)]
            public virtual int fee { get; set; }
            [Parameter("address", "params.recipient", 4)]
            public virtual string recipient { get; set; }
            [Parameter("uint256", "params.deadline", 5)]
            public virtual BigInteger deadline { get; set; }
            [Parameter("uint256", "params.amountIn", 6)]
            public virtual BigInteger amountIn { get; set; }
            [Parameter("uint256", "params.amountOutMinimum", 7)]
            public virtual BigInteger amountOutMinimum { get; set; }
            [Parameter("uint160", "params.sqrtPriceLimitX96", 8)]
            public virtual BigInteger sqrtPriceLimitX96 { get; set; }
        }


        public partial class swapExactTokensForTokensSupportingFeeOnTransferTokensFunction : swapExactTokensForTokensSupportingFeeOnTransferTokensFunctionBase { }

        [Function("swapExactTokensForTokensSupportingFeeOnTransferTokens", "uint256[]")]
        public class swapExactTokensForTokensSupportingFeeOnTransferTokensFunctionBase : FunctionMessage
        {
            [Parameter("uint256", "amountIn", 1)]
            public virtual BigInteger AmountIn { get; set; }
            [Parameter("uint256", "amountOutMin", 2)]
            public virtual BigInteger AmountOutMin { get; set; }
            [Parameter("address[]", "path", 3)]
            public virtual List<string> Path { get; set; }
            [Parameter("address", "to", 4)]
            public virtual string To { get; set; }
            [Parameter("uint256", "deadline", 5)]
            public virtual BigInteger Deadline { get; set; }
        }
    }
}
