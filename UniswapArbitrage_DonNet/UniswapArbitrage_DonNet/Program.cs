using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace UniswapArbitrage_DonNet
{
    class Program
    {
        static void Main(string[] args)
        {
            PairWethUsdc().Wait();
        }

        static async Task PairWethUsdc()
        {
            try
            {
                //Basic config
                int fund = 1;
                var privateKey = "YOUR KEY";
                var uniswap_pool_address = "0x45dda9cb7c25131df268515131f647d726f50608";
                var sushiswap_pool_address = "0x34965ba0ac2451A34a0471F04CCa3F990b8dea27";
                var uniSwap_fee = 0.005;
                var sushiswap_fee = 0.003;
                var benifit_rate = 0.003;

                //web3 account initial
                var account = new Account(privateKey);
                var web3 = new Web3(account, "https://polygon-rpc.com/");
                var gas = await web3.Eth.GasPrice.SendRequestAsync();

                var latestBlockNumber1 = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                //get uniswap v3 price
                var uniswapev3_weth_usdc_pair_contractHandler = web3.Eth.GetContractHandler(uniswap_pool_address);
                var uniswapev3_weth_usdc_pair_slot = await uniswapev3_weth_usdc_pair_contractHandler
                    .QueryDeserializingToObjectAsync<Slot0Function, Slot0OutputDTO>(new Slot0Function());
                //BigInteger price = 10 ^ 18 / 10 ^ 6 / ((sqrtPriceX96 / 2 ^ 96) ^ 2);
                double price_uniswape_weth = Math.Pow(10, 18) / Math.Pow(10, 6) / Math.Pow((double)uniswapev3_weth_usdc_pair_slot.SqrtPriceX96 / Math.Pow(2, 96), 2);

                //get sushiswap v2 price
                var sushiswapv2_weth_usdc_pair_contractHandler = web3.Eth.GetContractHandler(sushiswap_pool_address);
                var sushiswapv2_weth_usdc_pair_getReserves = await sushiswapv2_weth_usdc_pair_contractHandler
                        .QueryDeserializingToObjectAsync<GetReservesFunction, GetReservesOutputDTO>(new GetReservesFunction());
                var sushiswap_weth = (double)sushiswapv2_weth_usdc_pair_getReserves._reserve1 / (double)sushiswapv2_weth_usdc_pair_getReserves._reserve0;
                var adjusted_price_sushiswap_weth = sushiswap_weth / (Math.Pow(10, 18 - 6));
                var price_sushiswap_weth = 1 / adjusted_price_sushiswap_weth;
                //price = Reserve1/Reserve0
                //adjusted_price = price / (10 * *(18 - 6))
                //inverted_price = 1 / adjusted_price

                //check bockNumber
                var latestBlockNumber2 = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                if (latestBlockNumber1 != latestBlockNumber2)
                {
                    return;
                }

                //buy from sushiswap sell to uniswape
                if (price_uniswape_weth > price_sushiswap_weth)
                {
                    double earn_percent = (price_uniswape_weth - price_sushiswap_weth) / price_sushiswap_weth;
                    if (earn_percent > benifit_rate)
                    {
                        //buy from sushiswap
                        List<string> path = new List<string>();
                        path.Add("0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174");
                        path.Add("0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619");
                        double AmountOutMin = (1 / price_sushiswap_weth) * fund * 1000000000000000000 * (1 - sushiswap_fee) * 0.9999;
                        var sushiswapParams = new swapExactTokensForTokensSupportingFeeOnTransferTokensFunction
                        {
                            AmountIn = new BigInteger(1000000 * fund),
                            AmountOutMin = new BigInteger(AmountOutMin),
                            Path = path,
                            To = account.Address,
                            Deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(5).ToUnixTimeSeconds())
                        };
                        sushiswapParams.GasPrice = gas.Value * 4;

                        //sell to uniswap
                        var uniSwapParams = new ExactInputSingle
                        {
                            tokenIn = "0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619",
                            tokenOut = "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
                            fee = (int)(uniSwap_fee * 100000),
                            recipient = account.Address,
                            amountIn = new BigInteger(AmountOutMin * (1 - uniSwap_fee)),
                            deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(1000).ToUnixTimeSeconds()),
                            amountOutMinimum = 0,
                            sqrtPriceLimitX96 = 0
                        };
                        var uniswapeParams = new ExactInputSingleFunctionBase { Params = uniSwapParams };
                        uniswapeParams.GasPrice = gas.Value * 2;

                        //swap
                        var contract_sushiswap_swap = web3.Eth.GetContractHandler("0x1b02dA8Cb0d097eB8D57A175b88c7D8b47997506");
                        var contract_uniswap_swap = web3.Eth.GetContractHandler("0xe592427a0aece92de3edee1f18e0157c05861564");
                        var sushiswap_weth_swape = await contract_sushiswap_swap.SendRequestAsync(sushiswapParams);
                        var uniswap_weth_swape = await contract_uniswap_swap.SendRequestAsync(uniswapeParams);

                        var a = 0;
                    }
                }
                //buy from uniswape sell to sushiswap
                else if (price_sushiswap_weth > price_uniswape_weth)
                {
                    double earn_percent = (price_sushiswap_weth - price_uniswape_weth) / price_uniswape_weth;
                    if (earn_percent > benifit_rate)
                    {
                        //buy from uniswap
                        double AmountOutMin = (1 / price_uniswape_weth) * fund * 1000000000000000000 * (1 - uniSwap_fee);
                        var uniSwapParams = new ExactInputSingle
                        {
                            tokenIn = "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
                            tokenOut = "0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619",
                            fee = (int)(uniSwap_fee * 100000),
                            recipient = account.Address,
                            amountIn = new BigInteger(1000000 * fund),
                            deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(5).ToUnixTimeSeconds()),
                            amountOutMinimum = new BigInteger(AmountOutMin),
                            sqrtPriceLimitX96 = 0
                        };
                        var uniswapeParams = new ExactInputSingleFunctionBase { Params = uniSwapParams };
                        uniswapeParams.GasPrice = gas.Value * 3;

                        //sell to sushiswap
                        List<string> path = new List<string>();
                        path.Add("0x7ceB23fD6bC0adD59E62ac25578270cFf1b9f619");
                        path.Add("0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174");
                        var sushiswapParams = new swapExactTokensForTokensSupportingFeeOnTransferTokensFunction
                        {
                            AmountIn = new BigInteger(AmountOutMin * (1 - sushiswap_fee)),
                            AmountOutMin = 0,
                            Path = path,
                            To = account.Address,
                            Deadline = new BigInteger(DateTimeOffset.Now.AddSeconds(15).ToUnixTimeSeconds())
                        };
                        sushiswapParams.GasPrice = gas.Value * 2;

                        //swap
                        var contract_uniswap_swap = web3.Eth.GetContractHandler("0xe592427a0aece92de3edee1f18e0157c05861564");
                        var contract_sushiswap_swap = web3.Eth.GetContractHandler("0x1b02dA8Cb0d097eB8D57A175b88c7D8b47997506");
                        var uniswap_weth_swape = await contract_uniswap_swap.SendRequestAsync(uniswapeParams);
                        var sushiswap_weth_swape = await contract_sushiswap_swap.SendRequestAsync(sushiswapParams);

                        var a = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "";
            }
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
